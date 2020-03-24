using Appointment_Mgr.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Appointment_Mgr.ViewModel
{
    public class DoctorHomeViewModel : ViewModelBase
    {
        private DispatcherTimer timer;
        private string _greetingMessage, _doctorName;

        public string GreetingMessage 
        {
            get { return _greetingMessage; }
            set 
            {
                _greetingMessage = value;
                RaisePropertyChanged(nameof(GreetingMessage));
            }
        }


        public string DoctorName 
        {
            get { return _doctorName; }
            set 
            {
                _doctorName = value;
                RaisePropertyChanged(nameof(DoctorName));
            }
        }

        private int DoctorID { get; set; }

        private RelayCommand StartAppointmentCommand { get; set; }

        public DoctorHomeViewModel() 
        {
            if (IsInDesignMode)
            {
                GreetingMessage = "Good Morning,";
            }
            GreetingMessage = "Welcome Back,";

            timer = new DispatcherTimer(DispatcherPriority.Render);
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (sender, args) =>
            {
                GreetingMessage = getGreeting();
            };
            timer.Start();

            Messenger.Default.Register<int>(this, SetDoctorDetails);
            StartAppointmentCommand = new RelayCommand(StartAppointment);
        }

        public static string getGreeting()
        {
            TimeSpan morning = new TimeSpan(0, 0, 0);
            TimeSpan afternoon = new TimeSpan(12, 0, 0);
            TimeSpan now = DateTime.Now.TimeOfDay;

            if ((now > morning) && (now < afternoon))
            {
                return "Good Morning,";
            }
            else
            {
                return "Good Afternoon,";
            }
        }

        public void SetDoctorDetails(int msg) 
        {
            DoctorID = msg;
            DoctorName = StaffDBConverter.GetDoctorNameByID(DoctorID);
        }

        private void StartAppointment()
        {

            DataTable CheckedInPatients = PatientDBConverter.GetCheckedInAppointments();
            int averageDuration = AppointmentLogic.GetAverage();
            int remainingShiftInMinutes = StaffDBConverter.GetRemainingShiftInMinutes(DoctorID);

            if (remainingShiftInMinutes < averageDuration) 
            {
                //insert alert and return if answer is "NO"
            }
            


            /*
             * 
             * Calculate if doctor has enough time in shift for next appointment
             *      // if not, ask doctor to confirm they wish to take appointment
             * Fetch Appointment (to assign)
             *    {   
             *         IF Emergency is waiting:
             *              based on time emergency was created (in scenario there are several emergency appointments waiting) assign one waiting longest
             *              [this will impact reservation appointment wait time of the doctors]
             *         
             *         ELSE IF Doctor A has no RESERVATION appointment for him but there is a walk-in appointment for doctor B that doctor A can see early:
             *              asked if doctor A would like to take over next appointment --> gets reassigned from doctor B to doctor A
             *         
             *         ELSE IF Doctor has reservation waiting for him at same time (or within 10 minutes of) walk-in appointment, reservation appointment takes priority
             *                 and doctor is assigned reservation
             *        
             *         if there is no one waiting doctor is told no patients are checked in yet.
             *
             *    }
             */
            throw new NotImplementedException();
        }
    }
}
