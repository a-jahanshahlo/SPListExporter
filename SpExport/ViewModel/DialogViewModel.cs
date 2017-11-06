using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;

namespace SpExport.ViewModel
{
   public class DialogViewModel
    {
        // Variable
        private readonly IDialogCoordinator _dialogCoordinator;

        // Constructor
        public DialogViewModel(IDialogCoordinator instance)
        {                    
            _dialogCoordinator = instance;
        }

        // Methods
        public  async  void ShowMessage(string message,string header="")
        {
            await _dialogCoordinator.ShowMessageAsync(this, header,message);
        }

        public  async void ShowProgressMessage(string message, string header = "")
        {
            // Show...
            ProgressDialogController controller =await    _dialogCoordinator.ShowProgressAsync(this, header, message);
            controller.SetIndeterminate();
            
            // Do your work... 
             
            // Close...
            await controller.CloseAsync();
        }
        
    }
}
