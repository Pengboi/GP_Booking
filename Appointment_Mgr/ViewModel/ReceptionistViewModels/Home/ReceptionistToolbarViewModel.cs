using Appointment_Mgr.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
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
        private string _homeButtonTextColour, _managePatientButtonTextColour;
        private string _clockTime = DateTime.Now.ToString("HH:mm"); private string _dateValue = DateTime.Now.ToString("dd/MM/yy");
        private DispatcherTimer timer;

        public string HomeButtonTextColour 
        {
            get { return this._homeButtonTextColour; }
            set 
            {
                this._homeButtonTextColour = value;
                RaisePropertyChanged("HomeButtonTextColour");
            }
        }
        public string ManagePatientButtonTextColour
        {
            get { return this._managePatientButtonTextColour; }
            set
            {
                this._managePatientButtonTextColour = value;
                RaisePropertyChanged("ManagePatientButtonTextColour");
            }
        }

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

        public RelayCommand BookingCheckIn { private set; get; }
        public RelayCommand ManagePatient { private set; get; }

        public RelayCommand ExecuteLogout { private set; get; }


        public ReceptionistToolbarViewModel() 
        {
            if (IsInDesignMode)
            {
                LiveClock = "15:45";
                LiveDate = "05/09/16";
                UserLoggedIn = "Welcome, Lucy.";
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

            Messenger.Default.Register<NotificationMessage>(this, name => { UserLoggedIn = "Welcome, " + name.Notification + "."; });

            HomeButtonTextColour = "#40739e"; // Light blue hex code for selected navigation VM element
            ManagePatientButtonTextColour = "#2f3640"; // Dark Black for non-selected navigation VM element
            BookingCheckIn = new RelayCommand(SetBookingCheckInView);
            ManagePatient = new RelayCommand(SetManagePatientView);
            ExecuteLogout = new RelayCommand(ExecuteLogoutCommand);

        }

        public void SetBookingCheckInView() 
        {
            HomeButtonTextColour = "#40739e";
            ManagePatientButtonTextColour = "#2f3640";
            Messenger.Default.Send<string>("ReceptionistHomeView");
        }

        public void SetManagePatientView() 
        {
            HomeButtonTextColour = "#2f3640";
            ManagePatientButtonTextColour = "#40739e";
            Messenger.Default.Send<string>("ManagePatientView");
        }

        public void ExecuteLogoutCommand() 
        {
            Messenger.Default.Send<string>("HomeView");
        }

    }

}
