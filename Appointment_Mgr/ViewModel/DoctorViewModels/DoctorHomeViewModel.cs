using Appointment_Mgr.Dialog;
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
        private IDialogBoxService _dialogService;

        public DispatcherTimer timer;
        public string _greetingMessage, _doctorName;

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

        private void Alert(string title, string message)
        {
            var dialog = new AlertBoxViewModel(title, message);
            var result = _dialogService.OpenDialog(dialog);
        }
        private string Confirmation(string title, string message)
        {
            var dialog = new ConfirmationBoxViewModel(title, message);
            var result = _dialogService.OpenDialog(dialog);
            return result;
        }

        public int DoctorID { get; set; }

        public RelayCommand StartAppointmentCommand { get; set; }

        public DoctorHomeViewModel() 
        {
            _dialogService = new DialogBoxService();

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

            MessengerInstance.Register<int>(this, SetDoctorDetails);
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
            if (CheckedInPatients.Rows.Count == 0) 
            {
                Alert("No avaliable appointments.", "No patients are checked in. There are no appointments avalaible to be seen.");
                return;
            }

            int averageDuration = AppointmentLogic.GetAverage();
            int remainingShiftInMinutes = StaffDBConverter.GetRemainingShiftInMinutes(DoctorID);

            if (remainingShiftInMinutes < averageDuration) 
            {
                if (Confirmation("Are you sure?", "There may not be enough time in your schedule to finish the next appointment. Are you sure you would like to proceed?") == "NO")
                    return;
            }

            DataRow selectedAppointment = null;

            // check if any patients are waiting first.

            if (Convert.ToBoolean(CheckedInPatients.Rows[0]["isEmergency"]) == true)
            {
                selectedAppointment = CheckedInPatients.Rows[0];
            }

            foreach (DataRow dr in CheckedInPatients.Rows) 
            {
                if (selectedAppointment == null)
                {
                    if (Convert.ToBoolean(dr["isReservation"]) == false)
                        selectedAppointment = dr;
                    else if (Convert.ToBoolean(dr["isReservation"]) == true && DoctorID.Equals(int.Parse(dr["AppointmentDoctorID"].ToString())))
                    {
                        selectedAppointment = dr;
                        break;
                    }
                }
                else 
                {
                    if (Convert.ToBoolean(dr["isReservation"]) == true && DoctorID.Equals(int.Parse(dr["AppointmentDoctorID"].ToString()))) 
                    {
                        TimeSpan selectedAppointmentTime = TimeSpan.Parse(selectedAppointment["AppointmentTime"].ToString());
                        TimeSpan drAppointmentTime = TimeSpan.Parse(dr["AppointmentTime"].ToString());
                        if (drAppointmentTime.Subtract(selectedAppointmentTime).TotalMinutes < 10) 
                        {
                            selectedAppointment = dr;
                            break;
                        }
                    }

                }
            }

            PatientDBConverter.StartAppointment(selectedAppointment);
            MessengerInstance.Send<string>("DoctorAppointmentView");
        }
    }
}
