using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpExport.Core;

namespace SpExport.Model
{
    public class SpList//:TreeViewItemViewModel
    {
        public SpList()
           // : base(null, true)
        {
            Columns = new ObservableCollection<Column>();
        }
        public ObservableCollection<Column> Columns { get; set; }
        public Guid Id { get; set; }
        public string Title { get; set; }

        public string TypeName { get; set; }
        public string Icon { get; set; }
        public bool IsChecked { get; set; }
        //protected override void LoadChildren()
        //{
        //    foreach (Column state in Columns)
        //        base.Children.Add(state);
        //}

        public bool IsSupport { get; set; }
    }

    public class Column :ObjectBase// : TreeViewItemViewModel
    {
        private bool _isChecked;
        public string Title { get; set; }
        public string DisplayName { get; set; }
        public string InternalName { get; set; }
        public string Type { get; set; }

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (value!=_isChecked)
                {
                     _isChecked = value;
                    OnPropertyChanged(()=>IsChecked);
                }
               
            }
        }

        public bool Required { get; set; }
        public bool EnforceUniqueValue { get; set; }
        public string DefaultValue { get; set; }

        //public Column(TreeViewItemViewModel parent, bool lazyLoadChildren) : base(null, false)
        //{
        //}
    }
}
