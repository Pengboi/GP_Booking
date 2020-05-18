using Appointment_Mgr.Dialog;
using Appointment_Mgr.Model;
using Appointment_Mgr.Helper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Data;
using System.Windows.Input;

namespace Appointment_Mgr.ViewModel
{
    public class WalkInAppointmentViewModel : ViewModelBase
    {
        #region Popup boxes

        private IDialogBoxService _dialogService;

        public ICommand AlertCommand { get; private set; }
        public ICommand ErrorCommand { get; private set; }
        public ICommand SuccessCommand { get; private set; }

        private void Alert(string title, string message)
        {
            var dialog = new AlertBoxViewModel(title, message);
            var result = _dialogService.OpenDialog(dialog);
        }


        #endregion

        #region Viewmodel instance variables

        private int patientID;
        private string _estimatedTime;
        private DataRow _timeslot;

        public string EstimatedTime 
        {
            get { return _estimatedTime; }
            set 
            {
                _estimatedTime = value;
                RaisePropertyChanged(nameof(EstimatedTime));
            }
        }
        public DataRow Timeslot{ get; set; }
        public RelayCommand BookAppointmentCommand { get; set; }

        #endregion

        public WalkInAppointmentViewModel() 
        {
            _dialogService = new DialogBoxService();

            Timeslot = AppointmentLogic.CalcWalkInTimeslot();
            if (IsInDesignMode)
            {
                EstimatedTime = "2 Hours 45 Minutes";
            }
            else 
            {
                if (Timeslot == null) 
                {
                    EstimatedTime = "No Avaliable Time. Try booking a reservation.";
                }
                else
                {
                    EstimatedTime = CalcWaitTime();
                }
            }
            // When patientID message is received (from PatientDBConverter), set patient ID in VM.
            MessengerInstance.Register<double>(this, SetPatientID);

            BookAppointmentCommand = new RelayCommand(BookAppointment);
        }

        private void SetPatientID(double id) { patientID = (int)id; }

        public DataRow SelectTimeslot(DataTable dt) 
        {
            if (dt.Rows.Count <= 0) 
            {
                return null;
            }
            return dt.Rows[0];
        }

        public string CalcWaitTime() 
        {
            
            TimeSpan timeslot = TimeSpan.Parse(Timeslot[1].ToString());
            TimeSpan timeNow = DateTime.Now.TimeOfDay;

            TimeSpan timeDifference = timeslot - timeNow;

            string waitEstimation = "";

            if (timeDifference.Hours == 0)
                waitEstimation = timeDifference.Minutes.ToString() + " Minutes.";
            else
                waitEstimation = timeDifference.Hours.ToString() + " Hours. " + timeDifference.Minutes.ToString() + " Minutes.";

            return waitEstimation ;
        }

        public void BookAppointment() 
        {
            if (Timeslot == null) 
            {
                Alert("No Avaliability.", "No appointments are avaliable today. Please book a reservation appointment to be seen on another day.");
                return;
            }

            int doctorID = int.Parse(Timeslot[0].ToString());
            string timeslot = Timeslot[1].ToString();

            if (PatientDBConverter.PatientHasAppointment(patientID)) 
            {
                Alert("Appointment not booked", "You already have an appointment booked today. Please check your emails for notificaitons or speak to the receptionist.");
                MessengerInstance.Unregister(this);
                MessengerInstance.Send<string>("DecideHomeView");
                return;
            }

            PatientDBConverter.BookAppointment(timeslot, doctorID, patientID, false);
            AppointmentLogic.ScheduleWalkInNotification(TimeSpan.Parse(timeslot), patientID);

            var dialog = new SuccessBoxViewModel("Appointment Booked.",      //MOVE THIS
                            "Appointment has been successfully booked. Please keep an eye on your emails for updates on when we can see you.");
            var result = _dialogService.OpenDialog(dialog);

            MessengerInstance.Unregister(this);
            MessengerInstance.Send<string>("DecideHomeView");
        }
    }
}
