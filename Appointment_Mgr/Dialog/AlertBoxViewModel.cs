using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Appointment_Mgr.Dialog
{
    public class AlertBoxViewModel : DialogBoxViewModelBase<DialogResults>
    {
        public ICommand OKCommand { get; private set; }
        public string MessageImage { get; private set; }

        public AlertBoxViewModel() 
        {
            OKCommand = new RelayCommand<IDialogWindow>(OK);
            MessageImage = "pack://application:,,,/Assets/Attention-icon.png";

        }

        private void OK(IDialogWindow window) 
        {
            CloseDialogWithResult(window, DialogResults.Undefined);
        }
    }
}
