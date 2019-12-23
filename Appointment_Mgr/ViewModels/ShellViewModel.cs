using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private string _welcomeMessage = "Welcome";

        public string WelcomeMessage
        {
            get { return _welcomeMessage; }
            set 
            {
                _welcomeMessage = value;
                NotifyOfPropertyChange(() => WelcomeMessage); // Notify anyone who cares about _welcomeMessage that the property has been changed.
            }

        }



    }
}
