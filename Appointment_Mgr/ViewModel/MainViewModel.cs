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
        private string _clockTime = DateTime.Now.ToString("HH:mm"); private string _dateValue = DateTime.Now.ToString("dd/MM/yy");
        private string _userLogin = "Login";
        private ViewModelBase _currentViewModel;
        private DispatcherTimer timer;


        public string Title { get; set; }
        public string LiveClock
        {
            get { return this._clockTime; }
            set
            {
                if (this._clockTime == value)
                    return;
                this._clockTime = value;
                RaisePropertyChanged("LiveClock");
            }
        }
        public string LiveDate
        {
            get { return this._dateValue; }
            set
            {
                if (this._dateValue == value)
                    return;
                this._dateValue = value;
                RaisePropertyChanged("LiveDate");
            }
        }
        public string UserLogin
        {
            get { return this._userLogin; }
            set
            {
                this._userLogin = value;
                RaisePropertyChanged("UserLogin");
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
                LiveClock = "15:45";
                LiveDate = "05/09/16";
                UserLogin = _userLogin;

            }
            else
            {
                Title = "GP Booking";
                timer = new DispatcherTimer(DispatcherPriority.Render);
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += (sender, args) =>
                {
                    LiveClock = DateTime.Now.ToString("HH:mm");
                    LiveDate = DateTime.Now.ToString("dd/MM/yy");
                };
                timer.Start();
                UserLogin = _userLogin;

                CurrentViewModel = HomeVM;
            }
            ShowLoginCommand = new RelayCommand(LoginCommandMethod);
        }

        public RelayCommand ShowLoginCommand { private set; get; }
        public RelayCommand BookAppointmentCommand { get; private set; }

        public void LoginCommandMethod()
        {
            MessengerInstance.Send<NotificationMessage>(new NotificationMessage("LoginView"));
        }
    }
}