using GalaSoft.MvvmLight;
using System;
using System.ComponentModel;
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
        private string _greetingMessage = "Good " + getGreeting() + "."; private string _userLogin = "Login";
        public DispatcherTimer timer;

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
                GreetingMessage = "Placeholder";
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
                    GreetingMessage = getGreeting();
                };
                timer.Start();
                UserLogin = _userLogin;
            }
        }

        public string Title { get; set; }
        public string LiveClock
        {
            get { return this._clockTime; }
            set
            {
                if (_clockTime == value)
                    return;
                _clockTime = value;
                RaisePropertyChanged("LiveClock");
            }
        }
        public string LiveDate 
        {
            get { return this._dateValue; }
            set
            {
                if (_dateValue == value)
                    return;
                _dateValue = value;
                RaisePropertyChanged("LiveDate");
            }
        }
        public string GreetingMessage 
        {
            get { return this._greetingMessage; }
            set
            {
                if (_greetingMessage == "Good " + value + ".")
                    return;
                _greetingMessage = "Good " + value + ".";
                RaisePropertyChanged("GreetingMessage");
            }
        }
        public string UserLogin
        {
            get { return this._userLogin; }
            set 
            {
                _userLogin = value;
                RaisePropertyChanged("UserLogin");
            }
        }
        public static string getGreeting()
        {
            TimeSpan morning = new TimeSpan(10, 0, 0);
            TimeSpan afternoon = new TimeSpan(12, 0, 0);
            TimeSpan now = DateTime.Now.TimeOfDay;

            if ((now > morning) && (now < afternoon))
            {
                return "Morning";
            }
            else
            {
                return "Afternoon";
            }
        }

    }
}