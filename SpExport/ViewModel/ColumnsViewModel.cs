using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using SpExport.Core;

namespace SpExport.ViewModel
{
    public class ColumnsViewModel : ObjectBase
    {
        private readonly SPListViewModel _spListViewModel;
        private readonly MainViewModel _mainViewModel;


        public ColumnsViewModel(MainViewModel instance, SPListViewModel spListViewModel)
        {
            _mainViewModel = instance;

            _spListViewModel = spListViewModel;
        }

        public override bool Prev()
        {
            _mainViewModel.ConnectViewModel.RebindList();
            return base.Prev();
        }

        public override bool Next()
        {
            bool any = _spListViewModel.IsAnyColumnSelected();
            if (!any)
            {
                _mainViewModel.ShowMessage("Please select at least one column.");
                return false;
            }
            return base.Next();
        }
    }
}
