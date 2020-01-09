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
        private string _greetingMessage = "Good " + getGreeting() + ".";
        private DispatcherTimer timer;

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

        public HomeViewModel() 
        {
            if (IsInDesignMode)
            {
                GreetingMessage = "Placeholder";
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
            }
        }

    }
}
