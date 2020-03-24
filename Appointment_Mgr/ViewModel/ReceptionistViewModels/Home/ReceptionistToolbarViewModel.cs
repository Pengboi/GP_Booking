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
        private string _homeButtonTextColour, _managePatientButtonTextColour, _manageWaitingListButtonTextColour, _manageAppointmentsButtonTextColour;
        private string _clockTime = DateTime.Now.ToString("HH:mm"); private string _dateValue = DateTime.Now.ToString("dd/MM/yy");
        private DispatcherTimer timer;

        public string HomeButtonTextColour 
        {
            get { return this._homeButtonTextColour; }
            set 
            {
                this._homeButtonTextColour = value;
                RaisePropertyChanged(nameof(HomeButtonTextColour));
            }
        }
        public string ManageAppointmentsButtonTextColour
        {
            get { return this._manageAppointmentsButtonTextColour; }
            set
            {
                this._manageAppointmentsButtonTextColour = value;
                RaisePropertyChanged(nameof(ManageAppointmentsButtonTextColour));
            }
        }
        public string ManageWaitingListButtonTextColour
        {
            get { return this._manageWaitingListButtonTextColour; }
            set
            {
                this._manageWaitingListButtonTextColour = value;
                RaisePropertyChanged(nameof(ManageWaitingListButtonTextColour));
            }
        }
        public string ManagePatientButtonTextColour
        {
            get { return this._managePatientButtonTextColour; }
            set
            {
                this._managePatientButtonTextColour = value;
                RaisePropertyChanged(nameof(ManagePatientButtonTextColour));
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
        public RelayCommand ManageAppointments { private set; get; }
        public RelayCommand ViewCheckIn { private set; get; }
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
                timer = new DispatcherTimer(DispatcherPriority.Normal);
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
            ManageAppointmentsButtonTextColour = "#2f3640";
            ManageWaitingListButtonTextColour = "#2f3640";
            ManagePatientButtonTextColour = "#2f3640"; // Dark Black for non-selected navigation VM element
            BookingCheckIn = new RelayCommand(SetBookingCheckInView);
            ManageAppointments = new RelayCommand(SetManageAppointmentsView);
            ManagePatient = new RelayCommand(SetManagePatientView);
            ViewCheckIn = new RelayCommand(SetCheckInView);
            ExecuteLogout = new RelayCommand(ExecuteLogoutCommand);

        }

        public void SetBookingCheckInView() 
        {
            HomeButtonTextColour = "#40739e";
            ManageAppointmentsButtonTextColour = "#2f3640";
            ManageWaitingListButtonTextColour = "#2f3640";
            ManagePatientButtonTextColour = "#2f3640";
            MessengerInstance.Send<string>("ReceptionistHomeView");
            MessengerInstance.Unregister(this); // moves messenger to garbage collection
        }


        public void SetManageAppointmentsView()
        {
            HomeButtonTextColour = "#2f3640";
            ManageAppointmentsButtonTextColour = "#40739e";
            ManageWaitingListButtonTextColour = "#2f3640";
            ManagePatientButtonTextColour = "#2f3640";
            MessengerInstance.Send<string>("ManageAppointmentsView");
            MessengerInstance.Unregister(this); // moves messenger to garbage collection
        }

        public void SetCheckInView() 
        {
            HomeButtonTextColour = "#2f3640";
            ManageAppointmentsButtonTextColour = "#2f3640";
            ManageWaitingListButtonTextColour = "#40739e";
            ManagePatientButtonTextColour = "#2f3640";
            MessengerInstance.Send<string>("WaitingListView");
            MessengerInstance.Unregister(this); // moves messenger to garbage collection
        }


        public void SetManagePatientView() 
        {
            HomeButtonTextColour = "#2f3640";
            ManageAppointmentsButtonTextColour = "#2f3640";
            ManageWaitingListButtonTextColour = "#2f3640";
            ManagePatientButtonTextColour = "#40739e";
            MessengerInstance.Send<string>("ManagePatientView");
            MessengerInstance.Unregister(this); // moves messenger to garbage collection
        }

        public void ExecuteLogoutCommand() 
        {
            MessengerInstance.Send<string>("HomeView");
            MessengerInstance.Unregister(this); // moves messenger to garbage collection
            ViewModelLocator.Cleanup();
        }

    }

}
