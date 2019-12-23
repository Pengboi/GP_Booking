using Caliburn.Micro;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Appointment_Mgr.ViewModels;

namespace Appointment_Mgr
{
    public class Bootstrapper : BootstrapperBase
    {
        public Bootstrapper()
        {
            //Initialize will start up processes
            Initialize();
        }

        /// <summary>
        /// Overrides "OnStartup" method to display root view, defining startup window - in this case ShellViewModel
        /// Method is calling the ViewModel "ShellView" in ViewModels.
        /// Basically defining the onstartup window.
        /// 
        /// Upon startup, ShellViewModel is called - because of Caliburn.Micro, the View ShellView.xaml is opened
        /// instead of the ShellViewModel, due to Naming Conventions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
