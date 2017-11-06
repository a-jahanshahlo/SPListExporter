using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.SharePoint.Client;
using SpExport.Core;
using SpExport.Model;
using SpExport.Util.Command;
using SpExport.Util.Extention;
using SpExport.Util.Logger;

namespace SpExport.ViewModel
{
    [Serializable]
    public class ConnectionSql
    {

        [XmlElement("Server")]
        public string Server { get; set; }

        [XmlElement("Username")]
        public string Username { get; set; }
        [XmlElement("Password")]
        public string Password { get; set; }

    }
    public class SqlViewModel : ObjectBase
    {
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

        public string Server
        {
            get { return _server; }
            set
            {
                if (_server != value)
                {
                    _server = value;
                    _connectionSql.Server = value;

                    OnPropertyChanged(() => Server);
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
                    _username = value;
                    _connectionSql.Username = value;
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
                    _password = value;
                    _connectionSql.Password = value;

                    OnPropertyChanged(() => Password);

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
        public ObservableCollection<string> ExistDatabases { get; set; }

        private string _selectedDatabase;
        public string SelectedDatabase
        {
            get { return _selectedDatabase; }

            set
            {
                if (_selectedDatabase != value)
                {
                    _selectedDatabase = value;
                    OnPropertyChanged(() => SelectedDatabase);
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

        private readonly MainViewModel _mainViewModel;
        private ConnectionSql _connectionSql;
        public SqlViewModel(MainViewModel instance)
        {
            _mainViewModel = instance;

            _mainViewModel.PageChanged += page =>
            {
                if (page == Page.SpList)
                {
                    _connectionSql = base.DeSerialize<ConnectionSql>();
                    if (_mainViewModel.ConnectViewModel.IsSaveLogin)
                    {
                        Username = _connectionSql.Username;
                        Server = _connectionSql.Server;
                        Password = _connectionSql.Password;

                    }
                }
            };

            IsLoading = false;
            IsConnect = Visibility.Collapsed;
            ExistDatabases = new ObservableCollection<string>();
            _fetchDatabaseCommand = new Command(FetchDatabaseFunc);
            _worker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            _worker.DoWork += _worker_DoWork;
            _worker.RunWorkerCompleted += _worker_RunWorkerCompleted;
        }
        readonly BackgroundWorker _worker;
        private void FetchDatabaseFunc(object parameter)
        {
            if (_worker.IsBusy)
            {
                _mainViewModel.ShowMessage("Current thread is under running please wait a moment.");
                return;
            }
            PasswordBox passwordBox = (PasswordBox)parameter;
            this.Password = passwordBox.Password;
            _worker.RunWorkerAsync();

        }
        private readonly Command _fetchDatabaseCommand;
        private string _server = ".";
        private string _username = "sa";
        private string _password = "P@ssword";
        private Visibility _isConnect;
        private string _error;
        private bool _isLoading;

        private bool CheckLicense()
        { 
            var expire = new DateTime(2018, 9, 20);
            using (var cn = new SqlConnection(GetConnection))
            {
                cn.Open();
                using (var cmd = new SqlCommand("select getdate()", cn))
                {
                    var date = cmd.ExecuteScalar();
                    var serverdt = DateTime.Parse(date.ToString());
                    
                    if (serverdt > expire )
                    {
                        return false;

                    }

                }
            }
          
            if (DateTime.Now > expire)
            {
                return false;

            }
            return true;
        }
        public Command FetchDatabaseCommand
        {
            get { return _fetchDatabaseCommand; }
        }

        public bool IsExistTable(SpList list)
        {
            string constr = GetConnection;
            using (SqlConnection cn =
                new SqlConnection(constr))
            {
                cn.Open();
                using (var cmd = new SqlCommand(string.Format("Use [{0}] \n IF OBJECT_ID('[dbo].[{1}]', 'U') IS NULL select 1 else select 2",SelectedDatabase, list.Title), cn))
                {
                    var result = cmd.ExecuteScalar();
                    var exist = int.Parse(result.ToString());
                    if (exist == 1)
                    {
                        return false;
                    }

                        return true;
                  

                }
                 

            }
     
        }
        public List<string> DbExists { get; set; }
        private List<string> GetDatabaseList()
        {

            List<string> list = new List<string>();

            // Open connection to the database
            string conString = GetConnection;

            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();

                // Set up a command with the given query and associate
                // this with the current connection.
                using (SqlCommand cmd = new SqlCommand("SELECT name from sys.databases", con))
                {
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(dr[0].ToString());
                        }
                    }
                }
            }
            return list;

        }
        public void DropTable(SpList list)
        {
            string query = string.Format("Use [{0}] \n ", SelectedDatabase);

            query += string.Format("IF OBJECT_ID('[dbo].[{0}]', 'U') IS NOT NULL \n ", list.Title);
            query += "BEGIN \n ";
            query += string.Format("DROP TABLE [dbo].[{0}] \n ", list.Title);
            query += "END \n  ";

            string constr = GetConnection;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            Logs.Instance.Info(string.Format("Drop Table {0} ", list.Title));

        }
        public string GetConnection
        {
            get { return string.Format("server={0};uid={1};pwd={2};", Server, Username, Password); }
        }
        public void CreateTable(SpList list)
        {

            string query = string.Format("Use [{0}] \n ", SelectedDatabase);
            query += string.Format("IF OBJECT_ID('[dbo].[{0}]', 'U') IS NULL \n ", list.Title);
            query += "BEGIN \n ";
            query += string.Format("CREATE TABLE [dbo].[{0}](", list.Title);
            var cols = list.Columns.Where(x => x.IsChecked).ToList();
            if (cols.Count<=0)
            {
                return;
            }
            foreach (Column cl in cols)
            {
               
                query += string.Format("[{0}] {1} {2}  ,\n ", cl.InternalName, cl.Type.ToSqlDataType(), cl.Required.IsNull());//row

            }

            query += ")";
            query += " END \n ";

            string constr = GetConnection;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            Logs.Instance.Info(string.Format("Create Table {0} ", list.Title));
        }

        public void InsertToTable(SpList list, List<ListItem> items)
        {
            StringBuilder query = new StringBuilder();
            query.Append(string.Format("Use [{0}] \n ", SelectedDatabase));
            foreach (ListItem item in items)
            {

                StringBuilder[] values = new StringBuilder[2];
                values[0] = new StringBuilder();
                values[1] = new StringBuilder();

                query.Append(string.Format("INSERT INTO [dbo].[{0}](", list.Title));
                var cols = list.Columns.Where(x => x.IsChecked).ToList();

                foreach (Column column in cols)
                {

                    values[0].Append(string.Format(" [{0}] ,", column.InternalName));
                    values[1].Append(string.Format(" {0} ,", item[column.InternalName].GetSharepointFieldValue(column.Type)));

                }
                query.Append(values[0].ToString().TrimEnd(',') + " ) VALUES (" + values[1].ToString().TrimEnd(',') + ")");



                string constr = GetConnection;
                using (SqlConnection con = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand(query.ToString()))
                    {
                        cmd.Connection = con;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                }
                query.Clear();
                query.Append(string.Format("Use {0} \n ", SelectedDatabase));
            }

        }
        void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (DbExists == null)
            {
                return;
            }

            //App.Current.Dispatcher.Invoke(() => // <--- HERE
            //{
            ExistDatabases.Clear();

            foreach (string db in DbExists)
            {
                ExistDatabases.Add(db);
            }
            //});
        }

        void _worker_DoWork(object sender, DoWorkEventArgs e)
        {



            Error = string.Empty;
            IsLoading = true;
            IsConnect = Visibility.Hidden;
            try
            {

                DbExists = GetDatabaseList();



                Thread.Sleep(1000);
                IsConnect = Visibility.Visible;
            }
            catch (WebException ex)
            {
                Error = ex.Message;
                IsConnect = Visibility.Hidden;
                _worker.CancelAsync();
                Logs.Instance.Error(ex);


            }
            catch (Exception ex)
            {
                Error = ex.Message;
                IsConnect = Visibility.Hidden;
                _worker.CancelAsync();
                Logs.Instance.Error(ex);

            }
            finally
            {
                IsLoading = false;
            }

        }

        public override bool Prev()
        {
            SelectedDatabase = string.Empty;
            DbExists.Clear();
            return base.Prev();
        }

        public override bool Next()
        {
            base.Serialize(_connectionSql);

            if (!CheckLicense())
            {
                _mainViewModel.ShowMessage("Your License expired.");

                return false;
            }
            if (this.IsConnect != Visibility.Visible)
            {
                _mainViewModel.ShowMessage("Your are not connected to the SQL Server.");
                return false;
            }
            if (this.IsLoading == true)
            {
                _mainViewModel.ShowMessage("Sorry you can not move to the other pages until loading.");
                return false;
            }
            if (string.IsNullOrEmpty(SelectedDatabase))
            {
                _mainViewModel.ShowMessage("Please select Database from exist list.");
                return false;
            }

            return base.Next();
        }

        public string HelpTableName { get { return "HelpTableName"; } }
        public void DropHelpTable()
        {
            string query = string.Format("Use [{0}] \n ", SelectedDatabase);

            query += string.Format("IF OBJECT_ID('[dbo].[{0}]', 'U') IS NOT NULL \n ", HelpTableName);
            query += "BEGIN \n ";
            query += string.Format("DROP TABLE [dbo].[{0}] \n ", HelpTableName);
            query += "END \n  ";

            string constr = GetConnection;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
        }
        public void CreateHelpTable()
        {

            string query = string.Format("Use [{0}] \n ", SelectedDatabase);



            query += string.Format("IF OBJECT_ID('[dbo].[{0}]', 'U') IS NULL \n ", HelpTableName);
            query += "BEGIN \n ";
            query += string.Format("CREATE TABLE [dbo].[{0}](", HelpTableName);
            var cols = new[] { "TableName","Title", "InternalName", "DisplayName" };
            foreach (string cl in cols)
            {
                query += string.Format("[{0}] {1} {2}  ,\n ", cl, "nvarchar(400)", "not null");//row

            }

            query += ")";
            query += " END \n ";

            string constr = GetConnection;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
        }
        public void InsertToHelpTable(SpList list)
        {
            StringBuilder query = new StringBuilder();
            query.Append(string.Format("Use [{0}] \n ", SelectedDatabase));
            //foreach (ListItem item in items)
            //{

            StringBuilder values = new StringBuilder();
   

            var cols = list.Columns.Where(x => x.IsChecked).ToList();

            foreach (Column column in cols)
            {
                query.AppendLine();
                values.Clear();
                values.Append(string.Format("INSERT INTO [dbo].[{0}]( ", HelpTableName));
                values.Append("[TableName],[Title],[InternalName],[DisplayName] ");
                values.Append(" ) VALUES (");
                values.Append(string.Format(" N'{0}' ,", list.Title));
                values.Append(string.Format(" N'{0}' ,", column.Title));
                values.Append(string.Format(" N'{0}' ,", column.InternalName));
                values.Append(string.Format(" N'{0}' )", column.DisplayName));


                query.Append(values);
                query.AppendLine();

            }



            string constr = GetConnection;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query.ToString()))
                {
                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            query.Clear();
            query.Append(string.Format("Use {0} \n ", SelectedDatabase));
            //}

        }

    }
}
