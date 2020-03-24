using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;

namespace Appointment_Mgr.Dialog
{
    public class PatientIDBoxViewModel : DialogBoxViewModelBase<DialogResults>
    {
        public string _id = "";
        public ICommand OKCommand { get; private set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string ID
        {
            get { return this._id; }
            set
            {
                int isInt;
                if (!string.Equals(this._id, value))
                {
                    if (Int32.TryParse(value, out isInt))
                    {
                        this._id = value;
                    }
                    else
                    {
                        value = "";
                        return;
                    }
                }
            }
        }

        public PatientIDBoxViewModel(string title, string message) : base(title, message)
        {
            OKCommand = new RelayCommand<IDialogWindow>(OK);
            Title = title;
            Message = message;
            ID = _id;
        }

        private void OK(IDialogWindow window)
        {
            CloseDialogWithResult(window, ID);
        }
    }
}
