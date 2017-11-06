using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.SharePoint.Client;
using SpExport.Core;
using SpExport.Domain;
using SpExport.Model;
using SpExport.Util.Command;
using SpExport.Util.Cummonication;
using SpExport.Util.Extention;
using SpExport.Util.Logger;

namespace SpExport.ViewModel
{
    public class SPListViewModel : ObjectBase
    {
        private readonly ConnectViewModel _connect;
        private readonly MainViewModel _mainViewModel;

        public SPListViewModel(MainViewModel instance, ConnectViewModel connect)
        {
            Logs.Instance.Info("init SpList");

            _mainViewModel =instance;

            _connect = connect;
            Mediator.Instance.Register(BindData, ViewModelMessages.Connect);
            ExistList = new ObservableCollection<SpList>();
            GetSupportedLists = new ObservableCollection<SpList>();
            SelectedItemChangedCommand = new Command(SelectedChange);
          
            _worker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            _worker.DoWork += _worker_DoWork;
            _worker.RunWorkerCompleted += _worker_RunWorkerCompleted;
            Logs.Instance.Info("init SPList Complete");

        }

        public bool IsColumnLoading
        {
            get { return _isColumnLoading; }
            set
            {
                if (value != _isColumnLoading)
                {
                    _isColumnLoading = value;
                    OnPropertyChanged(() => IsColumnLoading);
                }
            }
        }

        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                App.Current.Dispatcher.Invoke(() => // <--- HERE
                {
                    Logs.Instance.Info("try to fetch sharepoint list columns");

                    IsColumnLoading = true;
                    foreach (SpList spList in GetSupportedLists.Where(x => x.IsChecked))
                    {
                        ClientContextSp clientContext = _connect.GetClientContextSp;
                        // clientContext.Credentials;// new NetworkCredential(txtUsername.Text.Trim(), txtPasssword.Password.Trim(), txtDomain.Text.Trim());

                        List oList = clientContext.Web.Lists.GetById(spList.Id);


                        var fields = oList.Fields;
                        clientContext.Load(oList);

                        clientContext.Load(fields);

                        clientContext.ExecuteQuery();
                        spList.Columns.Clear();
                        foreach (Field item in fields)
                        {
                            if (item.TypeAsString.IsValidTypeToConvert())
                            {


                                spList.Columns.Add(new Column //(null, true)
                                {
                                    DefaultValue = item.DefaultValue,
                                    DisplayName = item.TypeDisplayName,
                                    InternalName = item.InternalName,
                                    Type = item.TypeAsString,
                                    EnforceUniqueValue = item.EnforceUniqueValues,
                                    Required = item.Required,
                                    Title = item.Title
                                });
                            }
                            //Console.WriteLine(
                            //    "TypeAsString: {0} , Title: {1} ,  DefaultValue: {2} ,  Required: {3} , EnforceUniqueValues:{4}",
                            //    item.TypeAsString, item.Title, item.DefaultValue, item.Required, item.EnforceUniqueValues);
                        }
                    }
                    Logs.Instance.Info("end fetching SPList columns");
                });
            }
            catch (Exception ex)
            {

                _worker.CancelAsync();
                Logs.Instance.Error(ex);

            }
            finally
            {
                IsColumnLoading = false;

            }
        }

        private void BindData(object x)
        {
            App.Current.Dispatcher.Invoke(()=> // <--- HERE
            {

                Logs.Instance.Info("binding SPList to UI");

                var list = (ListCollection)x;
                ExistList.Clear();
                foreach (List lst in list)
                {
                    ExistList.Add(new SpList()
                    {
                        Id = lst.Id,
                        Title = lst.Title,
                        TypeName = lst.BaseTemplate.GetTemplateBaseName(),
                        IsSupport = lst.BaseTemplate.IsTemplateSupport()
                    });
                }
                Logs.Instance.Info("End SPList binding successfully");

            });
        }

        public ObservableCollection<SpList> ExistList { get; set; }

        public ObservableCollection<SpList> GetSupportedLists { get; set; }

        public Command SelectedItemChangedCommand { get; set; }

        private void SelectedChange()
        {

            //SpList spList = SelectedList.First(x => x.IsChecked);
            //List<ListItem> listItem = this.GetListData(spList.Id);
            // listItem.First().FieldValues.
        }

        private void BindColumns()
        {
            
            _worker.RunWorkerAsync();

        }

        public List<ListItem> GetListData(Guid listId, ref ListItemCollectionPosition position)
        {

            ClientContext clientContext = _connect.GetClientContextSp;
            List oList = clientContext.Web.Lists.GetById(listId);
            List<ListItem> items = new List<ListItem>();
            CamlQuery camlQuery = new CamlQuery
            {
                ViewXml =
                    "<View><Query><OrderBy><FieldRef Name=\"Created\" Ascending=\"false\" /></OrderBy></Query><RowLimit>100</RowLimit></View>"
            };




            //do
            //{
            ListItemCollection listItems = null;
            if (position != null)
            {
                camlQuery.ListItemCollectionPosition = position;
            }

            listItems = oList.GetItems(camlQuery);
            clientContext.Load(listItems);
            clientContext.ExecuteQuery();

            position = listItems.ListItemCollectionPosition;

            items.AddRange(listItems.ToList());
            //}
            //while (position != null);
            return items;

            //foreach (ListItem oListItem in collListItem)
            //{
            //    Console.WriteLine("ID: {0} \nTitle: {1} \nBody: {2}", oListItem.Id, oListItem["Title"], oListItem["Body"]);
            //}
        }

        public bool IsAnyColumnSelected()
        {
          return  GetSupportedLists.Any(x => x.Columns.Any(c => c.IsChecked));
     
        }
        public override bool Prev()
        {
            GetSupportedLists.Clear();
            ExistList.Clear();
            return base.Prev();
        }

        public override bool Next()
        {
            if (!ExistList.Any(x => x.IsChecked && x.IsSupport))
            {
                _mainViewModel.ShowMessage("Please select any supported list types.");
                return false;
            }
     
                GetSupportedLists.Clear();

                foreach (var spList in ExistList.Where(x => x.IsSupport && x.IsChecked))
                {
                    foreach (Column column in spList.Columns)
                    {
                        column.IsChecked = false;
                    
                    }
                    GetSupportedLists.Add(spList);
                }
                this.BindColumns();
          
            return base.Next();
        }
        readonly BackgroundWorker _worker;
        private bool _isColumnLoading;
    }
}
