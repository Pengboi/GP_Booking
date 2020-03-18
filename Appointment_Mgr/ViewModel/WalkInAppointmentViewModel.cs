using Appointment_Mgr.Dialog;
using Appointment_Mgr.Dialog.Error;
using Appointment_Mgr.Dialog.Confirmation;
using Appointment_Mgr.Model;
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
        public ICommand ConfirmationCommand { get; private set; }

        private void Error(string title, string message)
        {
            var dialog = new ErrorBoxViewModel(title, message);
            var result = _dialogService.OpenDialog(dialog);
        }
        private void Alert(string title, string message)
        {
            var dialog = new AlertBoxViewModel(title, message);
            var result = _dialogService.OpenDialog(dialog);
        }
        private void Confirmation(string title, string message)
        {
            var dialog = new ConfirmationBoxViewModel(title, message);
            var result = _dialogService.OpenDialog(dialog);
        }

        #endregion

        private string _estimatedTime;
        private DataRow _timeslot;

        public string EstimatedTime 
        {
            get { return _estimatedTime; }
            set 
            {
                _estimatedTime = value;
                RaisePropertyChanged("EstimatedTime");
            }
        }
        public DataRow Timeslot{ get; set; }
        public RelayCommand BookAppointmentCommand { get; set; }

        public WalkInAppointmentViewModel() 
        {
            _dialogService = new DialogBoxService();

            Timeslot = AppointmentLogic.CalcWalkInTimeslot(StaffDBConverter.GetWalkInTimeslots());
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
            BookAppointmentCommand = new RelayCommand(BookAppointment);
        }

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

            //int timeslot = int.Parse(Timeslot[1].ToString());
            //PatientDBConverter.BookAppointment(selectedTimeslot, reservationDoctorID, patientID, Comment, SelectedDate.ToShortDateString());

            MessengerInstance.Unregister(this);
            MessengerInstance.Send<string>("DecideHomeView");
        }
    }
}
