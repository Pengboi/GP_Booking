using Appointment_Mgr.Dialog;
using Appointment_Mgr.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Mgr.ViewModel
{
    public class EmergencyAppointmentViewModel: ViewModelBase
    {
        private IDialogBoxService _dialogService;

        public string Firstname { get; set; }
        public string Middlename { get; set; }
        public string Lastname { get; set; }
        public DateTime? DOB { get; set; }
        public string DoorNumber { get; set; }
        public string Postcode { get; set; }

        public RelayCommand ShowHomeView { private set; get; }
        public RelayCommand BookAppointmentCommand { private set; get; }

        #region DialogBox Definitions

        private void Success(string title, string message)
        {
            var dialog = new SuccessBoxViewModel(title, message);
            var result = _dialogService.OpenDialog(dialog);
        }
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

        #endregion

        #region ViewModel class

        public EmergencyAppointmentViewModel() 
        {
            _dialogService = new DialogBoxService();

            ShowHomeView = new RelayCommand(ShowHome);

            BookAppointmentCommand = new RelayCommand(BookAppointment);
        }

        private void ShowHome()
        {
            MessengerInstance.Send<string>("DecideHomeView");
        }

        #endregion

        #region ViewModel Class Methods

        private bool RequiredNotComplete()
        {
            if (string.IsNullOrWhiteSpace(Firstname) ||
                string.IsNullOrWhiteSpace(Lastname) ||
                !DOB.HasValue ||
                string.IsNullOrWhiteSpace(DoorNumber) ||
                string.IsNullOrWhiteSpace(Postcode)
               )
            {
                Alert("Required Fields not Complete", "Please complete all required fields in the patient details form." +
                      " If you are not a registered patient, please speak to the receptionist for further assistance.");
                return true;
            }
            return false;
        }

        public string VerifyPatientDetails(PatientUser p, int? id = null)
        {
            int recordsFound;
            if (id.HasValue)
                recordsFound = p.RecordsFound(id);
            else
                recordsFound = p.RecordsFound();
            if (recordsFound == 0)
            {
                Alert("Patient Not Found", "Patient record could not be found. Please speak to a receptionist to register as a patient with the GP");
                return "NoRecord";
            }
            else if (recordsFound > 1)
            {
                return "MultipleRecords";
            }
            else
                return "FoundRecord";
        }

        private void BookAppointment()
        {
            if (RequiredNotComplete())
                return;

            PatientUser patient = new PatientUser(Firstname, Middlename, Lastname, (DateTime)DOB, int.Parse(DoorNumber), Postcode);
            int patientID;

            // Verifies if patient records existing in patient DB, if multiple records are found, below situation is handelled using
            // appropirate dialog boxes. 
            string verifiedExistance = VerifyPatientDetails(patient);
            if (verifiedExistance.Equals("NoRecord"))
                return;
            // If multiple records are found, user is asked to input their patient ID to uniquely identify them.
            // If incorrect input format is detected i.e. non-numerical, or a patient ID not corrosponding to inputted details is entered,
            // the user is shown an alert message and redirected to partner with a receptionist for assistance.
            else if (verifiedExistance.Equals("MultipleRecords"))
            {
                Alert("Multiple Records Found.",
                      "Multple Records were found with your details. Please type in your Patient ID or speak to the receptionist for assistance with booking an appointment.");
                string inputtedID = PatientIDBox();
                if (string.IsNullOrWhiteSpace(inputtedID) || !inputtedID.All(char.IsDigit))
                {
                    Alert("Incorrect ID.", "Patient ID must be numerical. Please speak to a receptionist.");
                    return;
                }
                verifiedExistance = VerifyPatientDetails(patient, int.Parse(inputtedID));
                if (verifiedExistance.Equals("FoundRecord"))
                    patientID = int.Parse(inputtedID);
                else
                {
                    Alert("Could Not Find record.", "Could not find record matching details under inputted ID, please speak to a receptionist for assistance.");
                    return;
                }
            }
            else
                patientID = PatientDBConverter.GetPatientID(patient);

            if (PatientDBConverter.PatientHasAppointment(patientID)) 
            {
                Alert("Appointment Found!", "An existing appointment was found. Please cancel or check-in for your existing appointment to proceed.");
                return;
            }

            PatientDBConverter.BookEmergencyAppointment(patientID);
            Success("Appointment Booked.", "Emergency appointment has been booked. Please ask patient to be seated, the next avaliable doctor will take the session.");
            MessengerInstance.Send<string>("ReceptionistHomeView");
        }

        #endregion
    }
}
