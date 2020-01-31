using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Appointment_Mgr.Dialog.Confirmation
{
    public class ConfirmationBoxViewModel : DialogBoxViewModelBase<DialogResults>
    {

        public ICommand OKCommand { get; private set; }
        public string Message { get; set; }
        public string Title { get; set; }


        public ConfirmationBoxViewModel(string title, string message) : base(title, message)
        {
            OKCommand = new RelayCommand<IDialogWindow>(OK);
            Message = message;
            Title = title;
        }

        private void OK(IDialogWindow window)
        {
            CloseDialogWithResult(window, "OK");
        }
    }
}
