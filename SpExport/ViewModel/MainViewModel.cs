using SpExport.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using SpExport.Util.Command;
using SpExport.Util.Cummonication;
using SpExport.Util.Extention;

namespace SpExport.ViewModel
{
    public enum Page
    {
        ConnectSharepoint,
        SpList,
        SqlConnection
    }
    public delegate void ChangedPageEventHandler(Page page);
    public class MainViewModel : ObjectBase
    {
        private int _pageIndex;
        public event ChangedPageEventHandler PageChanged;
        protected virtual void OnChanged(Page p)
        {
            if (PageChanged != null)
                PageChanged(p);
        }
        public int PageIndex
        {
            get { return _pageIndex; }
            set
            {
                if (_pageIndex != value)
                {
                    _pageIndex = value;
                    OnPropertyChanged(() => PageIndex);
                }
            }
        }
        private bool _isOverrideTable;
        public bool IsOverrideTable
        {
            get { return _isOverrideTable; }
            set
            {
                if (_isOverrideTable != value)
                {
                    _isOverrideTable = value;
                    OnPropertyChanged(() => IsOverrideTable);
                }
            }
        }
        private bool _isVisiblePrev;
        public bool IsVisiblePrev
        {
            get { return _isVisiblePrev; }
            set
            {
                if (_isVisiblePrev != value)
                {
                    _isVisiblePrev = value;
                    OnPropertyChanged(() => IsVisiblePrev);
                }
            }
        }
        private bool _isVisibleNext;
        public bool IsVisibleNext
        {
            get { return _isVisibleNext; }
            set
            {
                if (_isVisibleNext != value)
                {
                    _isVisibleNext = value;
                    OnPropertyChanged(() => IsVisibleNext);
                }
            }
        }
        public MainViewModel(IDialogCoordinator instance)
        {
            _dialogCoordinator = instance;
            IsVisibleNext = true;
            Mediator.Instance.Register((x) =>
            {
                this.PageIndex = (int)x;
                if (PageIndex >= 1)
                {
                    IsVisiblePrev = true;
                }
                else
                {
                    IsVisiblePrev = false;
                }
            }, ViewModelMessages.IndexPage);
            this.PageIndex = 0;
            SqlViewModel = new SqlViewModel(this);
            ConnectViewModel = new ConnectViewModel(this);
            SpListViewModel = new SPListViewModel(this, ConnectViewModel);
            MonitorViewModel = new MonitorViewModel(this, SqlViewModel, ConnectViewModel, SpListViewModel);
            ColumnsViewModel = new ColumnsViewModel(this, SpListViewModel);
            ReadyViewModel = new ReadyViewModel(this);

            _finishCommand = new Command(FinishCommandFunc);
            _nextCommand = new Command(NextCommandFunc);
            _prevCommand = new Command(PrevCommandFunc);
            PageChanged += MainViewModel_PageChanged;
        }

        void MainViewModel_PageChanged(Page page)
        {

        }

        // Variable
        private readonly IDialogCoordinator _dialogCoordinator;

        // Constructor


        // Methods
        public async void ShowMessage(string message, string header = "")
        {
            await _dialogCoordinator.ShowMessageAsync(this, header, message);
        }

        public async Task<MessageDialogResult> ShowMessage(string message, string header, MetroDialogSettings dialogSettings)
        {
            return await _dialogCoordinator.ShowMessageAsync(this, header, message, MessageDialogStyle.AffirmativeAndNegative, dialogSettings);

        }
        public async void ShowProgressMessage(string message, string header = "")
        {
            // Show...
            ProgressDialogController controller = await _dialogCoordinator.ShowProgressAsync(this, header, message);
            controller.SetIndeterminate();

            // Do your work... 

            // Close...
            await controller.CloseAsync();
        }


        public DialogViewModel DialogViewModel { get; set; }
        public MonitorViewModel MonitorViewModel { get; set; }
        public ColumnsViewModel ColumnsViewModel { get; set; }
        public ConnectViewModel ConnectViewModel { get; set; }
        public SPListViewModel SpListViewModel { get; set; }
        public SqlViewModel SqlViewModel { get; set; }
        public ReadyViewModel ReadyViewModel { get; set; }
        #region Commands


        private readonly Command _nextCommand;

        public Command NextCommand
        {
            get { return _nextCommand; }
        }

        private void NextCommandFunc()
        {
            switch (PageIndex)
            {
                case 0:
                    if (this.ConnectViewModel.Next())
                    {
                        PageIndex++;
                        OnChanged(Page.ConnectSharepoint);
                    }
                    break;
                case 1:
                    if (this.SpListViewModel.Next())
                    {
                        PageIndex++;
                        OnChanged(Page.SpList);

                    }
                    break;
                case 2:
                    if (this.ColumnsViewModel.Next())
                    {
                        PageIndex++;
                    }
                    break;
                case 3:
                    if (this.SqlViewModel.Next())
                    {
                        PageIndex++;
                        OnChanged(Page.SqlConnection);

                    }
                    break;
                case 4:
                    if (this.ReadyViewModel.Next())
                    {
                        PageIndex++;
                    }
                    break;
                case 5:
                    if (this.MonitorViewModel.Next())
                    {
                        PageIndex++;
                    }
                    break;
            }
            if (PageIndex >= 1)
            {
                IsVisiblePrev = true;
            }
            else
            {
                IsVisiblePrev = false;
            }
        }

        private readonly Command _prevCommand;

        private void PrevCommandFunc()
        {
            if (PageIndex > 0)
            {
                PageIndex--;
                switch (PageIndex)
                {
                    case 0:
                        OnChanged(Page.ConnectSharepoint);
                        break;
                    case 1:
                        OnChanged(Page.SpList);
                        break;
                    case 2:
                        break;
                    case 3:
                        OnChanged(Page.SqlConnection);
                        break;
                    case 4:
                        break;
                    case 5:
                        break;
                }
            }
            if (PageIndex >= 1)
            {
                IsVisiblePrev = true;
            }
            else
            {
                IsVisiblePrev = false;
            }
        }
        public Command PrevCommand
        {
            get { return _prevCommand; }
        }
        private readonly Command _finishCommand;

        public Command FinishCommand
        {
            get { return _finishCommand; }
        }
        private void FinishCommandFunc() { }
        #endregion
    }
}
