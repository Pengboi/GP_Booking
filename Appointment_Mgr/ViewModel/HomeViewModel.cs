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
    public class HomeViewModel : ViewModelBase
    {
        public string _loggedIn = "";
        private string _greetingMessage = "Good " + getGreeting() + ".";
        private string _appointmentButtonImage = "pack://application:,,,/Assets/Book-icon.png", _checkInButtonImage = "pack://application:,,,/Assets/Clock-icon.png";
        private string _bookAppointment = "Book Appointment"; private string _checkIn = "Check In";
        private DispatcherTimer timer;

        public string BookAppointment
        {
            get { return this._bookAppointment; }
            set { _bookAppointment = value; }
        }
        public string AppointmentButtonImg
        {
            get { return this._appointmentButtonImage; }
            set { _appointmentButtonImage = value; }
        }

        public string CheckIn
        {
            get { return this._checkIn; }
            set { _checkIn = value; }
        }
        public string CheckInButtonImg
        {
            get { return this._checkInButtonImage; }
            set { _checkInButtonImage = value; }
        }

        public RelayCommand BookAppointmentCommand { private set; get; }

        public HomeViewModel()
        {
            AppointmentButtonImg = _appointmentButtonImage;
            CheckInButtonImg = _checkInButtonImage;

            if (IsInDesignMode)
            {
                GreetingMessage = "Placeholder";
                BookAppointment = "Placeholder";
                CheckIn = "Placeholder";
            }
            else
            {
                timer = new DispatcherTimer(DispatcherPriority.Render);
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += (sender, args) =>
                {
                    GreetingMessage = getGreeting();
                };
                timer.Start();
                BookAppointment = _bookAppointment;
                CheckIn = _checkIn;
            }

            BookAppointmentCommand = new RelayCommand(SetBookingView);
        }

        public static string getGreeting()
        {
            TimeSpan morning = new TimeSpan(0, 0, 0);
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

        public void SetBookingView()
        {
            //Notification Message to open Booking View
            MessengerInstance.Send<string>("BookingView");
            Cleanup();
        }


        public override void Cleanup()
        {
            Messenger.Default.Unregister(this);
            MessengerInstance.Unregister(this);
            base.Cleanup();
            ViewModelLocator.Cleanup();
        }
    }
}
