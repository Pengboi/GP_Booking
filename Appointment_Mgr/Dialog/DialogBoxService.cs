using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Mgr.Dialog
{
    public class DialogBoxService : IDialogBoxService
    {
        public string OpenDialog<T>(DialogBoxViewModelBase<T> viewModel)
        {
            IDialogWindow window = new DialogBoxView();
            window.DataContext = viewModel;
            window.ShowDialog();
            return viewModel.DialogResult;
        }

        public bool ShowMessage(string title, string message)
        {
            throw new NotImplementedException();
        }
    }
}
