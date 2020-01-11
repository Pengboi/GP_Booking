using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.ComponentModel;
using System.Windows.Threading;
using Appointment_Mgr.Model;
using System.Security;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;

namespace Appointment_Mgr.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class LoginViewModel : ViewModelBase
    {
        private string _username;
        private string _buttonText = "Sign in";
        
        ICommand AlertCommand { get; set; }
        ICommand ErrorCommand { get; set; }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public LoginViewModel() 
        {
            AlertCommand = new RelayCommand(ShowAlert);

            MessengerInstance.Register<NotificationMessage>(this, NotifyMe);
            if (IsInDesignMode) 
            {
                ButtonText = _buttonText;
            }
            else
            {
                ButtonText = _buttonText;
            }
            SignInClick = new RelayCommand(SignInValidation);
        }

        private void ShowAlert()
        {
            throw new NotImplementedException();
        }

        public string Username 
        {
            get { return _username; }
            set 
            {
                _username = value;
                RaisePropertyChanged(() => Username);
            }
        }
        public string Password { private get; set; }
        public string ButtonText 
        {
            get { return _buttonText; }
            set 
            {
                _buttonText = value;
            }
        }

        public RelayCommand SignInClick { private set; get; }

        public void NotifyMe(NotificationMessage notificationMessage)
        {
            string notification = notificationMessage.Notification;
            //do your work
        }

        public void SignInValidation()
        {
            StaffUser staffUser = new StaffUser(Username, Password);
            if (staffUser.userExists()) 
            {
                if (staffUser.verifyPassword()) 
                {
                    // Open 2 auth window
                }
                else
                    MessengerInstance.Send<NotificationMessage>(new NotificationMessage("Error401"));
            }
            else
                MessengerInstance.Send<NotificationMessage>(new NotificationMessage("Error404"));
        }
    }
}
