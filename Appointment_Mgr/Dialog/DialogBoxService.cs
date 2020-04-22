using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Mgr.Dialog
{
    // Inherits from Interface IDialogBoxService
    public class DialogBoxService : IDialogBoxService
    {
        public string OpenDialog<T>(DialogBoxViewModelBase<T> viewModel)
        {
            // Using IDialogWindow interface, instance of a dialog box is set
            // with the content presenter set to viewmodel of intended dialog box (of generic type T)
            // Window is then shown as a dialog box, meaning (other than async functions) main workflow 
            // code is not executed, until the results of the dialog box are returned and the box is closed.
            IDialogWindow window = new DialogBoxView();
            window.DataContext = viewModel;
            window.ShowDialog();
            return viewModel.DialogResult;
        }
    }
}
