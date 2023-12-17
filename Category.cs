using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptManager
{
    public class Category
    {
        public string CategoryName { get; set; }
        public ObservableCollection<ScriptItem> Items { get; set; }
    }
}
