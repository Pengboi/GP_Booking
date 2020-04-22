using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Mgr.Dialog
{
    public abstract class DialogBoxViewModelBase<T>
    {
        // ViewModelBase contains attributes all Dialog classes share in common
        // however is not a dialog box in of itself. Intended to be a base class
        // for all dialog boxes, indicating required attributes in dialog box, 
        // being Title, Message and Result (which is returned where not null)
        public DialogBoxViewModelBase(string title, string message)
        {
            Title = title;
            Message = message;
        }

        public string Title
        { get; set; }
        public string Message
        { get; set; }
        public string DialogResult 
        { get; set; }

        public void CloseDialogWithResult(IDialogWindow dialog, string result) 
        {
            DialogResult = result;

            if (dialog != null)
                dialog.DialogResult = true;
        }

    }
}
