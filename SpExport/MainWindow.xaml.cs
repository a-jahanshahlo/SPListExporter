using Microsoft.SharePoint.Client;
using SP = Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NLog.Config;
using SpExport.Domain;
using SpExport.Util.Logger;
using SpExport.ViewModel;

namespace SpExport
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        private const string ConsoleTargetName = "WpfConsole";
        private LoggingRule _loggingRule;

        ListCollection collList;
        private readonly MainViewModel _mainViewModel;
        public MainWindow()
        {
            InitializeComponent();
            _mainViewModel = new MainViewModel(DialogCoordinator.Instance);
            this.DataContext = _mainViewModel;
 
        }



        private void lstLists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
  
        }

        private void Wizard_CurrentPageChanged(object sender, AvalonWizard.CurrentPageChangedEventArgs e)
        {
   
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var consoleTarget = Logs.GetConsoleTarget(ConsoleTargetName, TextLog, Name);
            var targetWrapper = Logs.GetConsoleWrapper(ConsoleTargetName, consoleTarget);
            _loggingRule = Logs.ReloadConsole(targetWrapper);

            Logs.Instance.Info("Load windows");
        }

 


    }
}
