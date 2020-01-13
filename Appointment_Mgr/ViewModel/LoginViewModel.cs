using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.ComponentModel;
using System.Windows.Threading;
using Appointment_Mgr.Model;
using System.Security;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using Appointment_Mgr.Dialog;

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
        private Dialog.IDialogBoxService _dialogService;

        public ICommand AlertCommand { get; private set; }
        public ICommand ErrorCommand { get; private set; }
        public ICommand OtpCommand { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public LoginViewModel() 
        {
            ErrorCommand = new RelayCommand(Error);
            OtpCommand = new RelayCommand(Otp);
            _dialogService = new DialogBoxService();

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

        private void Otp()
        {
            var dialog = new Dialog.OTP.OTPBoxViewModel("", "Input your OTP code below:");
            var result = _dialogService.OpenDialog(dialog);
        }

        private void Error()
        {
            throw new NotImplementedException();
        }

        private void Alert(string title, string message)
        {
            var dialog = new AlertBoxViewModel(title, message);
            var result = _dialogService.OpenDialog(dialog);
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
                    string otpCode = staffUser.getOTP();
                    Otp();
                }
                else
                    Alert("Password Incorrect", "Incorrect password. Please try again. If issues persist, please contact" +
                        " the IT administrator or speak to a member of HR.");
            }
            else
            {
                Alert("User Not Found", "The account could not be found. Please check your username & try again. If issues" +
                    " persist, please contact the IT administrator or speak to a member of HR.");
            }
        }
    }
}
