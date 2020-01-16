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

        public string UserLoggedIn { get; set; }

        public RelayCommand ExecuteLogout { private set; get; }


        public ReceptionistToolbarViewModel() 
        {
            if (IsInDesignMode)
            {
                LiveClock = "15:45";
                LiveDate = "05/09/16";
            }
            else
            {
                timer = new DispatcherTimer(DispatcherPriority.Render);
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += (sender, args) =>
                {
                    LiveClock = DateTime.Now.ToString("HH:mm");
                    LiveDate = DateTime.Now.ToString("dd/MM/yy");
                };
                timer.Start();
            }

            Messenger.Default.Register<string>(this, name => { UserLoggedIn = "Welcome, " + name + "."; });
            ExecuteLogout = new RelayCommand(ExecuteLogoutCommand);

        }


        public void ExecuteLogoutCommand() 
        {
            Messenger.Default.Send<string>("HomeView");
        }

    }

}
