using Appointment_Mgr.Model;
using Appointment_Mgr.Dialog;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;

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
                if (this._userLogin == "logout")
                {
                    //code that opens home toolbar VM here
                }
                else
                {
                    //code that opens receptionist or doctor toolbar VM here
                }
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
                    CurrentToolbarViewModel = ReceptionistToolbarVM;
                }
                else if (userAccount.isDoctor()) 
                {
                    //open Doctor toolbar here
                }
            }
        }

        public RelayCommand BookAppointmentCommand { get; private set; }

    }
}