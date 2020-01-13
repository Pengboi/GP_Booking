using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Mgr.Dialog
{
    public abstract class DialogBoxViewModelBase<T>
    {

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
            Console.WriteLine(result); //DEBUG

            if (dialog != null)
                dialog.DialogResult = true;
        }

    }
}
