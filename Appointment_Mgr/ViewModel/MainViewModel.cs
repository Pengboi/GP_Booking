using Appointment_Mgr.Helper;
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
        private ViewModelBase _currentViewModel, _currentToolbarViewModel; //maybe not needed? - idk
        public RelayCommand BookAppointmentCommand { get; private set; }
        public string Title { get; set; }
 
        public string UserLogin
        {
            get { return this._userLogin; }
            set
            {
                this._userLogin = value;
                RaisePropertyChanged(nameof(UserLogin));
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

        public ViewModelBase HomeToolbarVM { get { return ViewModelLocator.HomeToolbar; } }
        public ViewModelBase HomeVM { get { return ViewModelLocator.Home; } }
        public ViewModelBase ReceptionistToolbarVM { get { return ViewModelLocator.ReceptionistToolbar; } }
        public ViewModelBase ReceptionistVM { get { return ViewModelLocator.ReceptionistHome; } }
        public ViewModelBase ManageAppointmentsVM { get { return ViewModelLocator.ManageAppointments; } }
        public ViewModelBase ManageWaitListVM { get { return ViewModelLocator.WaitingList; } }
        public ViewModelBase LoginVM { get { return ViewModelLocator.Login; } }
        
        public ViewModelBase BookingVM { get { return ViewModelLocator.BookAppointment; } }
        public ViewModelBase EmergencyBookingVM { get { return ViewModelLocator.BookEmergencyAppointment; } }
        public ViewModelBase CheckInVM { get { return ViewModelLocator.CheckIn; } }

        public ViewModelBase ManagePatientVM { get { return ViewModelLocator.ManagePatient; } }

        public ViewModelBase DoctorVM { get { return ViewModelLocator.DoctorHome; } }
        public ViewModelBase DoctorToolbarVM { get { return ViewModelLocator.DoctorToolbar; } }
        

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
            _ = CancelTardyAppointments.Start();
        }

        private void ReceiveLoginMessage(StaffUser userAccount)
        {
            if (userAccount.isReceptionist())
            {
                string firstname = userAccount.getFirstname();
                ViewModelLocator.Cleanup();
                CurrentToolbarViewModel = ReceptionistToolbarVM;
                CurrentViewModel = ReceptionistVM;
                MessengerInstance.Send<NotificationMessage>(new NotificationMessage(firstname));  // FROM: MainVM TO: ReceptionistToolbarVM ~ sends logged in users first name.
            }
            else if (userAccount.isDoctor())
            {
                //open Doctor toolbar here
                CurrentToolbarViewModel = DoctorToolbarVM;
                CurrentViewModel = DoctorVM;
                MessengerInstance.Send<int>(StaffDBConverter.GetAccountIDByUsername(userAccount.getUsername()));
            }
        }

        // Respondsible for resetting view to home / alter view when notification message is sent by other 
        // classes containing any of the values below
        private void ChangeView(string value) 
        {
            ViewModelLocator.Cleanup();

            if (value == "LoginView") 
            {
                CurrentViewModel = LoginVM;
            }
            if (value == "HomeView")
            {
                CurrentViewModel = HomeVM;
                CurrentToolbarViewModel = HomeToolbarVM;
                ViewModelLocator.Cleanup();
            }
            if (value == "DecideHomeView") 
            {
                if (CurrentToolbarViewModel.GetType().ToString() == "Appointment_Mgr.ViewModel.ReceptionistToolbarViewModel")
                {
                    CurrentViewModel = ReceptionistVM;
                }
                else if (CurrentToolbarViewModel.GetType().ToString() == "Appointment_Mgr.ViewModel.HomeToolbarViewModel") 
                {
                    CurrentViewModel = HomeVM;
                    CurrentToolbarViewModel = HomeToolbarVM;
                }
            }

            if (value == "BookingView")
                CurrentViewModel = BookingVM;
            if (value == "EmergencyBookingView")
                CurrentViewModel = EmergencyBookingVM;

            if (value == "ReceptionistHomeView")
                CurrentViewModel = ReceptionistVM;

            if (value == "ManageAppointmentsView")
                CurrentViewModel = ManageAppointmentsVM;

            if (value == "WaitingListView")
                CurrentViewModel = ManageWaitListVM;

            if (value == "ManagePatientView")
                CurrentViewModel = ManagePatientVM;

            if (value == "CheckInView")
                CurrentViewModel = CheckInVM;

            if (value == "DoctorHomeView")
                CurrentViewModel = DoctorVM;
        }

        

    }
}