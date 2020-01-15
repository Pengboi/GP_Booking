using Appointment_Mgr.Model;
using Appointment_Mgr.Dialog;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Ioc;

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
    public class MainViewModel : ViewModelBase
    {
        private string _userLogin = "Login";
        private ViewModelBase _currentViewModel, _currentToolbarViewModel;

        public string Title { get; set; }
 
        public string UserLogin
        {
            get { return this._userLogin; }
            set
            {
                this._userLogin = value;
                RaisePropertyChanged("UserLogin");
            }
        }

        public ViewModelBase CurrentToolbarViewModel 
        {
            get { return _currentToolbarViewModel; }
            set 
            {
                _currentToolbarViewModel = value;
                RaisePropertyChanged(() => CurrentToolbarViewModel);
            }
        }

        public ViewModelBase CurrentViewModel 
        {
            get { return _currentViewModel; }
            set 
            {
                _currentViewModel = value;
                RaisePropertyChanged(() => CurrentViewModel);
            }
        }

        public ViewModelBase HomeToolbarVM 
        {
            get { return (ViewModelBase)ViewModelLocator.HomeToolbar; }
        }

        public ViewModelBase ReceptionistToolbarVM { get { return (ViewModelBase)ViewModelLocator.ReceptionistToolbar; } }

        public ViewModelBase HomeVM 
        {
            get { return (ViewModelBase)ViewModelLocator.Home; }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                Title = "GP Booking (Design Mode)";
                UserLogin = _userLogin;

            }
            else
            {
                Title = "GP Booking";
                UserLogin = _userLogin;
                CurrentToolbarViewModel = HomeToolbarVM;
                CurrentViewModel = HomeVM;
            }
            Messenger.Default.Register<StaffUser>
            (
                 this,
                 (action) => ReceiveLoginMessage(action)
            );
            MessengerInstance.Register<string>(this, ChangeView);
        }

        private void ReceiveLoginMessage(StaffUser userAccount)
        {
            if (userAccount.getUsername() == "logout")
            {
                //logout code here
            }
            else 
            {
                if (userAccount.isReceptionist())
                {
                    string firstname = userAccount.getFirstname();
                    ViewModelLocator.Cleanup();
                    CurrentToolbarViewModel = ReceptionistToolbarVM;
                    MessengerInstance.Send<string>(firstname);
                }
                else if (userAccount.isDoctor()) 
                {
                    //open Doctor toolbar here
                }
            }
        }

        // Respondsible for resetting view to home / alter view when notification message is sent by other 
        // classes containing any of the values below
        private void ChangeView(string value) 
        {
            Console.WriteLine("Got here");
            Console.WriteLine(value);

            if (value == "HomeView") 
            {
                CurrentViewModel = HomeVM;
            }
                
            if (value == "HomeToolbarView") 
            {
                Messenger.Reset(); //RESETS MESSENGER SETTINGS --> FIXES BUG
                //Re-add the Messengers defined in the constructor which have been cleared
                Messenger.Default.Register<StaffUser>
                (
                    this,
                    (action) => ReceiveLoginMessage(action)
                );
                MessengerInstance.Register<string>(this, ChangeView);

                CurrentToolbarViewModel = HomeToolbarVM;
                ViewModelLocator.Cleanup();
            }
                

        }

        public RelayCommand BookAppointmentCommand { get; private set; }

    }
}