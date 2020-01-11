using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Mgr.Dialog
{
    public abstract class DialogBoxViewModelBase<T>
    {
        public DialogBoxViewModelBase() : this(string.Empty, string.Empty) { }
        public DialogBoxViewModelBase(string title) : this(title, string.Empty) { }
        public DialogBoxViewModelBase(string title, string message)
        {
            Title = title;
            Message = message;
            
        }

        public string Title
        { get; set; }
        public string Message
        { get; set; }
        public T DialogResult 
        { get; set; }

        public void CloseDialogWithResult(IDialogWindow dialog, T result) 
        {
            DialogResult = result;

            if (dialog != null)
                dialog.DialogResult = true;
        }

    }
}
