using SpExport.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml.Serialization;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.SharePoint.Client;
using NLog.Fluent;
using SpExport.Domain;
using SpExport.Util.Command;
using SpExport.Util.Cummonication;
using SpExport.Util.Extention;
using SpExport.Util.Logger;

namespace SpExport.ViewModel
{
    [Serializable]
    public class ConnectionInfo
    {

        [XmlElement("SiteUrl")]
        public string SiteUrl { get; set; }
        [XmlElement("Domain")]
        public string Domain { get; set; }
        [XmlElement("Username")]
        public string Username { get; set; }
        [XmlElement("Password")]
        public string Password { get; set; }
        [XmlElement("IsSaveLogin")]

        public bool IsSaveLogin { get; set; }
    }
    public class ConnectViewModel : ObjectBase
    {

        private string _siteUrl;// = "http://app.dev.local/";
        private string _domain;// = "dev.local";
        private string _username;// = "administrator";
        private string _password;// = "P@ssword123";
 
        public bool IsSaveLogin
        {
            get { return _isSaveLogin; }
            set
            {
                if (_isSaveLogin != value)
                {
                    _isSaveLogin = value;
                    _connectionInfo.IsSaveLogin = value;
                    OnPropertyChanged(() => IsSaveLogin);
                }
            }
        }
        public string Error
        {
            get { return _error; }
            set
            {
                if (_error != value)
                {
                    _error = value;
                    OnPropertyChanged(() => Error);
                }
            }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                if (value != _isLoading)
                {
                    _isLoading = value;
                    OnPropertyChanged(() => IsLoading);
                }
            }
        }

        public Visibility IsConnect
        {
            get { return _isConnect; }
            set
            {
                if (_isConnect != value)
                {
                    _isConnect = value;
                    OnPropertyChanged(() => IsConnect);
                }
            }
        }

        public string SiteUrl
        {
            get { return _siteUrl; }
            set
            {
                if (_siteUrl != value)
                {
                    _siteUrl = value;
                    IsConnect = Visibility.Hidden;
                    _connectionInfo.SiteUrl = value;

                    OnPropertyChanged(() => SiteUrl);
                }
            }
        }

        public string Domain
        {
            get { return _domain; }
            set
            {

                if (_domain != value)
                {
                    IsConnect = Visibility.Hidden;
                    _domain = value;
                    _connectionInfo.Domain = value;

                    OnPropertyChanged(() => Domain);
                }
            }
        }

        public string Username
        {
            get { return _username; }
            set
            {

                if (_username != value)
                {
                    IsConnect = Visibility.Hidden;
                    _username = value;
                    _connectionInfo.Username = value;

                    OnPropertyChanged(() => Username);
                }
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (_password != value)
                {
                    IsConnect = Visibility.Hidden;
                    _password = value;
                    _connectionInfo.Password = value;
                    OnPropertyChanged(() => Password);
                }
            }
        }

        private readonly Command _connectCommand;
        private Visibility _isConnect;
        private string _error;
        private bool _isLoading;
 
        public Command ConnectCommand
        {
            get { return _connectCommand; }
        }
        private readonly MainViewModel _mainViewModel;
        private readonly ConnectionInfo _connectionInfo;
        private ListCollection collList = null;
        public ClientContextSp GetClientContextSp
        {
            get
            {
                ClientContextSp clientContextSp = this.BuildClientContextSp(this.SiteUrl);
                clientContextSp.Credentials = new NetworkCredential(this.Username.Trim(), this.Password.Trim(), this.Domain.Trim());

                return clientContextSp;
            }
        }

        public ConnectViewModel()
        {

        }

        public void RebindList()
        {
            if (collList!=null)
            {
                Mediator.Instance.NotifyColleagues(ViewModelMessages.Connect, collList);
                
            }
        }
        public ConnectViewModel(MainViewModel instance)
        {
            Logs.Instance.Info("init connection");

            _connectionInfo = base.DeSerialize<ConnectionInfo>();
            if (_connectionInfo.IsSaveLogin)
            {
                Username = _connectionInfo.Username;
                Domain = _connectionInfo.Domain;
                SiteUrl = _connectionInfo.SiteUrl;
                IsSaveLogin = _connectionInfo.IsSaveLogin;

            }

            // Password = _connectionInfo.Password;

            _mainViewModel = instance;

            _worker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            _worker.DoWork += _worker_DoWork;
            _worker.RunWorkerCompleted += _worker_RunWorkerCompleted;
            _connectCommand = new Command(Connect);
            IsConnect = Visibility.Collapsed;
            IsLoading = false;
            Logs.Instance.Info("init connection view model complete");

        }

        void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {


        }

        void _worker_DoWork(object sender, DoWorkEventArgs e)
        {

            Logs.Instance.Info("try to connect Sharepoint site");

            Error = string.Empty;
            IsLoading = true;

            ClientContextSp clientContext = this.GetClientContextSp;
            Web oWebsite = clientContext.Web;
            collList = oWebsite.Lists;

            try
            {
                Thread.Sleep(1000);
                clientContext.Load(collList);

                clientContext.ExecuteQuery();
 
                base.Serialize(_connectionInfo);


                IsConnect = Visibility.Visible;
                Mediator.Instance.NotifyColleagues(ViewModelMessages.Connect, collList);
                Mediator.Instance.NotifyColleagues(ViewModelMessages.IndexPage, 1);
                Logs.Instance.Info("Connected to Sharepoint site successfuly");

            }
            catch (WebException ex)
            {
                Error = ex.Message;
                Logs.Instance.Error(ex);

            }
            catch (Exception ex)
            {
                Error = ex.Message;
                Logs.Instance.Error(ex);

            }
            finally
            {
                IsConnect = Visibility.Hidden;

                IsLoading = false;
                _worker.CancelAsync();

            }

        }

        private void Connect(object parameter)
        {



            // _deserializedObject = _deserializer.DeserializeObject(_sourceData);
            if (_worker.IsBusy)
            {
                _mainViewModel.ShowMessage("Current thread is under running please wait a moment.");
                return;
            }
            PasswordBox passwordBox = (PasswordBox)parameter;
            this.Password = passwordBox.Password;
            _worker.RunWorkerAsync();

        }

        private ClientContextSp BuildClientContextSp(string url)
        {
            return new ClientContextSp(url);
        }

        readonly BackgroundWorker _worker;//= new BackgroundWorker();
        private bool _isSaveLogin;

        public override bool Prev()
        {

            return base.Prev();
        }
        public override bool Next()
        {
            if (this.IsConnect != Visibility.Visible)
            {
                var msg = "Your are not connected to sharepoint site.";

                _mainViewModel.ShowMessage(msg);
                return false;
            }
            if (this.IsLoading == true)
            {
                var msg = "Sorry you can not move to the other pages until loading.";
                _mainViewModel.ShowMessage(msg);

                return false;

            }
            return base.Next();
        }
    }
}
