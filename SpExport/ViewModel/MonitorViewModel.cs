using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.SharePoint.Client;
using SpExport.Core;
using SpExport.Model;
using SpExport.Util.Command;
using SpExport.Util.Logger;

namespace SpExport.ViewModel
{
    public class MonitorViewModel : ObjectBase
    {
        private readonly SqlViewModel _sqlViewModel;
        private ConnectViewModel _connectViewModel;
        private readonly SPListViewModel _spListViewModel;
        private ListItemCollectionPosition position;
        private readonly MainViewModel _mainViewModel;
        private readonly BackgroundWorker _worker;
        private Command _cancelTransferCommand;
        private bool _isBusy;

       
        public MonitorViewModel(MainViewModel instance, SqlViewModel sqlViewModel, ConnectViewModel connectViewModel, SPListViewModel spListViewModel)
        {
            _mainViewModel = instance;
            IsBusy = false;
            _sqlViewModel = sqlViewModel;
            _connectViewModel = connectViewModel;
            _spListViewModel = spListViewModel;
            position = null;
            _worker = new BackgroundWorker
  {
      WorkerSupportsCancellation = true,
      WorkerReportsProgress = true
  };
            _worker.DoWork += _worker_DoWork;
            _worker.RunWorkerCompleted += _worker_RunWorkerCompleted;
            
            CancelTransferCommand = new Command(CancelTransfer);
            _transferCommand = new Command(TransferCommandFunc);
        }

        private async  void CancelTransfer()
        {
            var result =await  _mainViewModel.ShowMessage("Are you sure to cancel transfering process?", "Cacel process",
                new MetroDialogSettings() {AffirmativeButtonText = "Yes", NegativeButtonText = "No"});
            if (result == MessageDialogResult.Affirmative)
            {
                _worker.CancelAsync();
                IsBusy = false;
                
            }
        }
        private readonly Command _transferCommand;

        public Command TransferCommand
        {
            get { return _transferCommand; }
        }

        private void TransferCommandFunc()
        {
            StartTransfer();
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (value != _isBusy)
                {
                    _isBusy = value;
                    OnPropertyChanged(() => IsBusy);
                }
            }
        }

        public Command CancelTransferCommand
        {
            get { return _cancelTransferCommand; }
            set
            {
                if (_cancelTransferCommand != value)
                {
                    _cancelTransferCommand = value;
                    OnPropertyChanged(() => CancelTransferCommand);
                }
            }
        }

        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;

        }

        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Logs.Instance.Info("Transfering data to SQL Server began");

            IsBusy = true;
            try
            { 
                _sqlViewModel.DropHelpTable();
                 _sqlViewModel.CreateHelpTable();
              //  Thread.Sleep(15000);
                foreach (SpList spList in _spListViewModel.GetSupportedLists)
                {

                    if (!spList.Columns.Any(x => x.IsChecked))
                    {
                       
                        continue;
                    }
                    _sqlViewModel.InsertToHelpTable(spList);
                    if (_worker.CancellationPending)
                    {
                        Logs.Instance.Info("Cancel data transfering.");

                        return;
                    }

                    Logs.Instance.Info(string.Format("start to transfer data from {0} list.",spList.Title));
                    if (_sqlViewModel.IsExistTable(spList) && !_mainViewModel.IsOverrideTable) // donot override
                    {
                        Logs.Instance.Info(string.Format("skip table [{0}] and prevent to override.", spList.Title));
                        continue;
                    }
                    _sqlViewModel.DropTable(spList);

                    _sqlViewModel.CreateTable(spList);
               

                    do
                    {
                        if (_worker.CancellationPending)
                        {
                            Logs.Instance.Info("Cancel data transfering.");

                            return;
                        }
                        List<ListItem> items = _spListViewModel.GetListData(spList.Id, ref position);
                    
                        Logs.Instance.Info(string.Format("Fetch {0} records from {1} . ", items.Count,spList.Title));

                        _sqlViewModel.InsertToTable(spList, items);
                        Logs.Instance.Info(string.Format("Inserted {0} records from {1}. ", items.Count,spList.Title));

                    } while (position != null);
                    Logs.Instance.Info(string.Format( "Data transfering from {0} complete",spList.Title));

                }
            }
            catch (Exception ex)
            {
                _worker.CancelAsync();
                Logs.Instance.Error( ex);

            }
            finally
            {
                IsBusy = false;
            }
            Logs.Instance.Info("All data transfer to SQL Server successfully");

        }

        public void StartTransfer()
        {
            if (_worker.IsBusy)
            {
                _mainViewModel.ShowMessage("Current thread is under running please wait a moment.");
                return;
            }
            _worker.RunWorkerAsync();
        }

    }
}
