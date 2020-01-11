using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.ComponentModel;
using System.Windows.Threading;
using Appointment_Mgr.Model;
using System.Security;
using GalaSoft.MvvmLight.Command;

namespace Appointment_Mgr.Dialog
{
    public class DialogBoxViewModel : ViewModelBase
    {

        public DialogBoxViewModel(string title, string message) 
        {
            Title = title;
            Message = message;
            ButtonClick = new RelayCommand(CloseDialog);
        }

        public void CloseDialog() 
        {
            MessengerInstance.Send<NotificationMessage>(new NotificationMessage("CloseDialog"));
        }

        public string Title 
        { get; set; }
        public string Message
        { get; set; }
        public string MessageImage 
        { get; set; }
        public string ButtonText
        { get; set; }
        public RelayCommand ButtonClick { private set; get; }
    }
}
