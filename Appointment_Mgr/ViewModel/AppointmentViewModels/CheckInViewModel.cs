using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Appointment_Mgr.Model;
using Appointment_Mgr.Dialog;
using System.Windows.Input;
using System.Data;

namespace Appointment_Mgr.ViewModel
{
    public class CheckInViewModel : ViewModelBase
    {
        public DateTime? DOB { get; set; }
        public string Firstname { get; set; }
        public string DoorNo { get; set; }
        public string Postcode { get; set; }

        public RelayCommand CheckInCommand { private set; get; }
        public RelayCommand ShowHomeView { private set; get; }

        private Dialog.IDialogBoxService _dialogService;
        public ICommand AlertCommand { get; private set; }

        private void Alert(string title, string message)
        {
            var dialog = new AlertBoxViewModel(title, message);
            var result = _dialogService.OpenDialog(dialog);
        }
        private string PatientIDBox() 
        {
            var dialog = new PatientIDBoxViewModel("", "Type in your Patient ID:");
            var result = _dialogService.OpenDialog(dialog);

            return result;
        }
        private void Success(string title, string message)
        {
            var dialog = new SuccessBoxViewModel(title, message);
            var result = _dialogService.OpenDialog(dialog);
        }

        public CheckInViewModel() 
        {
            _dialogService = new DialogBoxService();

            ShowHomeView = new RelayCommand(ShowHome);
            CheckInCommand = new RelayCommand(CheckIn);
        }

        private bool RequiredNotComplete()
        {
            if (string.IsNullOrWhiteSpace(Firstname) ||
                !DOB.HasValue ||
                string.IsNullOrWhiteSpace(DoorNo) ||
                string.IsNullOrWhiteSpace(Postcode)
               )
            {
                return true;
            }
            return false;
        }
        private void ShowHome()
        {
            MessengerInstance.Send<string>("DecideHomeView");
        }
        private void CheckIn()
        {
            if (RequiredNotComplete()) 
            {
                Alert("Complete Required Fields!", "Please complete all fields to check in.");
                return;
            }

            DataTable patientRecords = PatientDBConverter.GetAwaitingCheckIn((DateTime)DOB, Firstname, DoorNo, Postcode);
            if (patientRecords.Rows.Count > 1)
            {
                Alert("Could not check in!", "More than one patient has an appointment with the same details provided. Please enter you patient ID or ask a recpetionist" +
                    " for assistance with check-in.");
                string input = PatientIDBox();
                if (string.IsNullOrWhiteSpace(input) || !input.All(char.IsDigit)) 
                {
                    Alert("Incorrect ID.", "Patient ID must be numerical. Please speak to a receptionist.");
                    return;
                }
                DataRow[] foundPatient = patientRecords.Select("PatientID = '" + input + "'");
                if (foundPatient.Length != 0)
                {
                    foreach (DataRow row in foundPatient)
                    {
                        int appointmentID = int.Parse(row["AppointmentID"].ToString());
                        PatientDBConverter.CheckInPatient(appointmentID);
                    }
                }
                else
                {
                    Alert("Could not check in!", "Incorrect Patient ID has been inputted. Please speak to the receptions for assitance with check-in.");
                    return;
                }

            }
            else if (patientRecords.Rows.Count == 0)
            {
                Alert("Could not check in!", "Unable to find appointment with details provided. Please confirm details or speak to receptionist. " +
                    "If you are late another appointment may be required.");
                return;
            }
            else 
            {
                int appointmentID = int.Parse(patientRecords.Rows[0][0].ToString());
                PatientDBConverter.CheckInPatient(appointmentID);
            }
            Success("Checked-In Successfully", "Please take a seat. Your doctor will see you soon.");
            ShowHome();
        }
    }
}
