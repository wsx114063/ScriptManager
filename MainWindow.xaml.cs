using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace ScriptManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string scriptFolderPath = @"Script";
        private string defaultEncodingList = "EncodingList.json";
        ObservableCollection<Category> Categories = new ObservableCollection<Category>();
        public MainWindow()
        {
            InitializeComponent();
            GetDefaultFolder();
            GetEncodingType();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveData("ScriptItems.json");
        }

        private void GetEncodingType()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var defaultEncodingList = "EncodingList.json";
            if (File.Exists(defaultEncodingList))
            {
                using (var reader = new StreamReader(defaultEncodingList, detectEncodingFromByteOrderMarks: true))
                {
                    var encodeList = JsonSerializer.Deserialize<List<string>>(reader.ReadToEnd());
                    foreach (var item in encodeList)
                    {
                        cmbEncoding.Items.Add(item);
                    }

                    if (FindIndexOfItem(cmbEncoding, Encoding.UTF8.WebName) == -1)
                    {
                        cmbEncoding.Items.Add(Encoding.UTF8.WebName);
                    }

                    cmbEncoding.SelectedValue = Encoding.UTF8.WebName;
                }
            }
            else
            {
                foreach (var item in Encoding.GetEncodings().OrderBy(x => x.Name))
                {
                    Encoding e = item.GetEncoding();
                    cmbEncoding.Items.Add(e.WebName);
                }

                cmbEncoding.SelectedValue = Encoding.UTF8.WebName;
            }
        }

        private void CmbEncoding_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (lstScripts.SelectedItem != null && cmbEncoding.SelectedItem is string selectedEncoding)
                {
                    var item = (ScriptItem)lstScripts.SelectedItem;
                    Encoding encoding = Encoding.GetEncoding(selectedEncoding);
                    using (var reader = new StreamReader(item.FilePath, encoding))
                    {
                        txtScriptContent.Text = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"oops! 選擇編碼方式後，出現了意外{ex.Message}");
            }

        }

        private void GetDefaultFolder()
        {
            lstScripts.Items.Clear();
            var defaultScriptPath = "ScriptItems.json";
            if (File.Exists(defaultScriptPath))
            {
                using (var reader = new StreamReader(defaultScriptPath, detectEncodingFromByteOrderMarks: true))
                {
                    var scriptList = JsonSerializer.Deserialize<List<ScriptItem>>(reader.ReadToEnd());
                    foreach (var item in scriptList)
                    {
                        lstScripts.Items.Add(item);
                    }
                }
            }
            else
            {
                foreach (var filePath in Directory.GetFiles(scriptFolderPath))
                {
                    var fileName = Path.GetFileName(filePath);
                    lstScripts.Items.Add(new ScriptItem { FilePath = filePath, FileName = fileName });

                }
            }
        }

        private void BtnLoadScripts_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Multiselect = true; // 允許選擇多個文件
            openFileDialog.Filter = "Script files (*.sql;*.txt;*.bat;*.cs;*.vb)|*.sql;*.ps1;*.bat|All files (*.*)|*.*"; // 設置文件過濾器
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); // 初始目錄

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filePath in openFileDialog.FileNames)
                {
                    var scriptItem = new ScriptItem
                    {
                        FileName = Path.GetFileName(filePath),
                        FilePath = filePath
                    };

                    lstScripts.Items.Add(scriptItem);
                }
            }
        }

        private void BtnClearScripts_Click(object sender, RoutedEventArgs e)
        {
            lstScripts.Items.Clear();
        }

        private void LstScripts_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (lstScripts.SelectedItem != null && cmbEncoding.SelectedItem is string selectedEncoding)
            {
                var item = (ScriptItem)lstScripts.SelectedItem;
                Encoding encoding = Encoding.GetEncoding(selectedEncoding);
                using (var reader = new StreamReader(item.FilePath, encoding))
                {
                    txtScriptContent.Text = reader.ReadToEnd();
                }
            }
            else if (lstScripts.SelectedItem != null)
            {
                if (lstScripts.SelectedItem is ScriptItem selectedScript)
                {
                    try
                    {
                        var filePath = selectedScript.FilePath;
                        using (var reader = new StreamReader(filePath, detectEncodingFromByteOrderMarks: true))
                        {
                            txtScriptContent.Text = reader.ReadToEnd();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Oh! 選擇腳本後出現意外，訊息如下: \n{ex.Message}");
                    }
                }
            }
        }

        private void MoveItem(int direction)
        {
            try
            {
                if (lstScripts.SelectedItem != null)
                {
                    // 檢查是否有選擇項目
                    if (lstScripts.SelectedItem == null || lstScripts.Items.Count < 2) return;

                    int newIndex = lstScripts.SelectedIndex + direction;

                    // 檢查新索引是否在有效範圍內
                    if (newIndex < 0 || newIndex >= lstScripts.Items.Count) return;

                    object selected = lstScripts.SelectedItem;
                    lstScripts.Items.Remove(selected);
                    lstScripts.Items.Insert(newIndex, selected);
                    lstScripts.SelectedIndex = newIndex;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"oops! 項目移動時，出現了意外{ex.Message}");
            }
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            MoveItem(-1);
        }
        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            MoveItem(1);
        }

        private void SaveData(string savePath)
        {
            var scriptItems = lstScripts.Items.OfType<ScriptItem>().ToList();
            var json = JsonSerializer.Serialize(scriptItems, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(savePath, json);

            var cmbItems = cmbEncoding.Items.OfType<string>().ToList();
            json = JsonSerializer.Serialize(cmbItems, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(defaultEncodingList, json);
        }

        private int FindIndexOfItem(ComboBox comboBox, string itemToFind)
        {
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                if (comboBox.Items[i].ToString() == itemToFind)
                {
                    return i;
                }
            }
            return -1; // 如果未找到，返回 -1
        }
    }
}
