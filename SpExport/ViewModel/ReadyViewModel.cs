using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using SpExport.Core;

namespace SpExport.ViewModel
{
    public class ReadyViewModel : ObjectBase
    {
        private bool _acceptTerm;

        public bool AcceptTerm
        {
            get { return _acceptTerm; }
            set
            {
                if (value != _acceptTerm)
                {
                    _acceptTerm = value;
                    OnPropertyChanged(() => AcceptTerm);
                }
            }
        }
        private readonly MainViewModel _mainViewModel;


        public ReadyViewModel(MainViewModel instance)
        {
            _mainViewModel = instance;

            AcceptTerm = false;
        }

        public override bool Next()
        {
            if (!AcceptTerm)
            {
                _mainViewModel.ShowMessage("Please accept term before to continue.");
                return false;
            }
            return base.Next();
        }
    }
}
