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
using OtpNet;

using Appointment_Mgr.Helper;

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
        private IDialogBoxService _dialogService;

        public ICommand AlertCommand { get; private set; }
        public ICommand ErrorCommand { get; private set; }
        public ICommand OtpCommand { get; private set; }

        public RelayCommand SignInClick { private set; get; }
        public RelayCommand CloseClick { private set; get; }

        private string Otp()
        {
            var dialog = new Dialog.OTP.OTPBoxViewModel("", "Input your OTP code below:");
            var result = _dialogService.OpenDialog(dialog);
            return result;
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

        

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public LoginViewModel() 
        {
            ErrorCommand = new RelayCommand(Error);
            _dialogService = new DialogBoxService();

            if (IsInDesignMode) 
            {
                ButtonText = _buttonText;
            }
            else
            {
                ButtonText = _buttonText;
            }
            SignInClick = new RelayCommand(SignInValidation);
            CloseClick = new RelayCommand(CloseView);
        }


        public void SignInValidation()
        {
            StaffUser staffUser = new StaffUser(Username, Password);
            if (staffUser.userExists())
            {
                if (staffUser.verifyPassword())
                {
                    //check if account is Doctor OR Receptionist --> Otherwise trigger alert
                    if (!(staffUser.isDoctor() || staffUser.isReceptionist())) 
                    {
                        Alert("Account Not Authorised", "Your account credentials are not authorised to access" +
                            " this system.");
                        return;
                    }

                    string inputtedCode = Otp();

                    string otpToken = staffUser.getOTP();
                    var bytes = Base32Encoding.ToBytes(otpToken);
                    var totp = new Totp(bytes);
                    var totpCode = totp.ComputeTotp();
                    
                    if (totpCode == inputtedCode)
                    {
                        //Returns user signed in to MainViewModel
                        Console.WriteLine("Verified user");
                        Messenger.Default.Send<StaffUser>(new StaffUser(staffUser.getUsername(), ""));
                        
                    }
                    else 
                    {
                        Alert("One-Time Password Incorrect", "The inputted code is incorrect. Please verify your TOTP and " +
                            "retry. If issues persist, please contact the IT administrator or speak to a member of HR.");
                    }
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

        public void CloseView() 
        {
            // Closes Login View
            // FROM: LoginViewModel TO: MainViewModel
            Messenger.Default.Send<string>("HomeView");
        }
    }
}
