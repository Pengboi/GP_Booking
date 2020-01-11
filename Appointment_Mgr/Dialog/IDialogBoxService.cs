using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Mgr.Dialog
{
    public interface IDialogBoxService
    {
        T OpenDialog<T>(DialogBoxViewModelBase<T> viewModel);
        //bool ShowMessage(string title, string message, MessageType messageType);
    }
}
