using Appointment_Mgr.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class ReceptionistToolbarViewModel : ViewModelBase
    {
        private string _clockTime = DateTime.Now.ToString("HH:mm"); private string _dateValue = DateTime.Now.ToString("dd/MM/yy");
        private DispatcherTimer timer;

        public string UserLoggedIn { get; set; }

        public RelayCommand ExecuteLogout { private set; get; }


        public ReceptionistToolbarViewModel() 
        {
            Messenger.Default.Register<string>(this, name => { UserLoggedIn = "Welcome, " + name + "."; });
            ExecuteLogout = new RelayCommand(ExecuteLogoutCommand);
        }


        public void ExecuteLogoutCommand() 
        {
            Messenger.Default.Send<string>("HomeView");
        }

    }

}
