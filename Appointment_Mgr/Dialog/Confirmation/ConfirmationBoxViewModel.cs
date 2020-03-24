using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Appointment_Mgr.Dialog
{
    public class ConfirmationBoxViewModel : DialogBoxViewModelBase<DialogResults>
    {
        public ICommand YesCommand { get; private set; }
        public ICommand NoCommand { get; private set; }
        public string Message { get; set; }
        public string Title { get; set; }


        public ConfirmationBoxViewModel(string title, string message) : base(title, message)
        {
            YesCommand = new RelayCommand<IDialogWindow>(Yes);
            NoCommand = new RelayCommand<IDialogWindow>(No);
            Message = message;
            Title = title;
        }

        private void Yes(IDialogWindow window)
        {
            CloseDialogWithResult(window, "Yes");
        }
        private void No(IDialogWindow window)
        {
            CloseDialogWithResult(window, "No");
        }
    }
}
