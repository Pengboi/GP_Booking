using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Appointment_Mgr.ViewModels
{
    /// <summary>
    /// Inheriting from screen allows for control over opening / closing events.
    /// I.E. in event where form is updated but not yet saved, screen would allow for
    /// warning to enduser if they would like to save before exiting - JUST ONE OF MANY EXAMPLES
    /// </summary>
    public class ShellViewModel : Screen
    {   
        // naming convention : start with _ then camelCase. These variables should not be manipulated directly
        private string _greetingMessage = "Good " + getGreeting() + "."; private string _loginMessage = "Login";
        private string _liveClock = DateTime.Now.ToString("HH:mm"); private string _liveDate = DateTime.Now.ToString("dd/MM/yy");

        public string GreetingMessage
        {
            get { return _greetingMessage; }
            set 
            {
                _greetingMessage = value;
                NotifyOfPropertyChange(() => GreetingMessage); // Notify anyone who cares about _welcomeMessage that the property has been changed.
            }

        }

        public string LoginMessage 
        {
            get { return _loginMessage; }
            set
            {
                _loginMessage = value;
                NotifyOfPropertyChange(() => LoginMessage);
            }

        }

       public string LiveClock 
       {
            get { return _liveClock; }
            set { _liveClock = value; }
       }

        public string LiveDate
        {
            get { return _liveDate; }
            set { _liveDate = value; }
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
        void updateTimeComponents(object sender, EventArgs e)
        {
            LiveClock = DateTime.Now.ToString("HH:mm");
            LiveDate = DateTime.Now.ToString("dd/MM/yy");
            GreetingMessage = "Good " + getGreeting() + ".";
        }

        public ShellViewModel() 
        {
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render);
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += updateTimeComponents;
            timer.Start();

        }

        

    }
}
