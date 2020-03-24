using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Appointment_Mgr.Model;
using Appointment_Mgr.Dialog;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

namespace Appointment_Mgr.ViewModel
{
    public class BookAppointmentViewModel : ViewModelBase
    {
        // Patient capture visibility is defined by width. If width = 0, it is hidden, else it is shown
        private bool _isBookingVisible;
        private string _patientCaptureWidth = "*", _bookingWidth = "*";
        
        private string _bookingSubviewVisible = "Hidden";

        public ViewModelBase _appointmentTypeViewModel;
        public ViewModelBase ReservationView { get { return (ViewModelBase)ViewModelLocator.ReservationAppointment; } }
        public ViewModelBase WalkInView { get { return (ViewModelBase)ViewModelLocator.WalkInAppointment; } }

        public RelayCommand WalkInCommand { private set; get; }
        public RelayCommand ReservationCommand { private set; get; }
        public RelayCommand ShowPatientCapture { private set; get; }
        public RelayCommand ShowHomeView { private set; get; }

        public string Firstname { get; set; }
        public string Middlename { get; set; }
        public string Lastname { get; set; }
        public DateTime? DOB { get; set; }
        public string Email { get; set; }
        public string StreetNo { get; set; }
        public string Postcode { get; set; }

        public string PatientCaptureWidth
        {
            get { return _patientCaptureWidth; }
            set
            {
                _patientCaptureWidth = value;
                RaisePropertyChanged(nameof(PatientCaptureWidth));
            }
        }
        public bool IsBookingVisible
        {
            get { return _isBookingVisible; }
            set
            {
                _isBookingVisible = value;
                RaisePropertyChanged(nameof(IsBookingVisible));
            }
        }
        public string BookingWidth
        {
            get { return _bookingWidth; }
            set
            {
                _bookingWidth = value;
                RaisePropertyChanged(nameof(BookingWidth));
            }
        }
        public string BookingSubviewVisible
        {
            get { return _bookingSubviewVisible; }
            set
            {
                _bookingSubviewVisible = value;
                RaisePropertyChanged(nameof(BookingSubviewVisible));
            }
        }

        private IDialogBoxService _dialogService;

        

        //Dialog box definitions
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

        //
        public ViewModelBase AppointmentTypeView
        {
            get { return _appointmentTypeViewModel; }
            set
            {
                _appointmentTypeViewModel = value;
                RaisePropertyChanged(() => AppointmentTypeView);
            }
        }

        public BookAppointmentViewModel()
        {
            _dialogService = new DialogBoxService();

            WalkInCommand = new RelayCommand(ShowWalkInView);
            ReservationCommand = new RelayCommand(ShowReservationView);

            BookingSubviewVisible = "Hidden";
            AppointmentTypeView = ReservationView;
            ShowPatientCapture = new RelayCommand(ShowPatientGrid);
            ShowHomeView = new RelayCommand(ShowHome);
        }

        private bool RequiredNotComplete()
        {
            if (string.IsNullOrWhiteSpace(Firstname)||
                string.IsNullOrWhiteSpace(Lastname) ||
                !DOB.HasValue                       ||
                string.IsNullOrWhiteSpace(Email)    ||
                string.IsNullOrWhiteSpace(StreetNo) ||
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

        public void ShowReservationView() 
        {
            if (RequiredNotComplete())
                return;

            PatientUser patient = new PatientUser(Firstname, Middlename, Lastname, (DateTime)DOB, int.Parse(StreetNo), Postcode);
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

            PatientDBConverter.UpdateEmail(patientID, Email);
            // Shows the booking view after patient details & desired reservation type verified
            // sends patient user details as message to view
            AppointmentTypeView = ReservationView;
            IsBookingVisible = true;
            BookingSubviewVisible = "Visible";
            PatientCaptureWidth = "0";
            MessengerInstance.Send<double>(patientID);
        }
        public void ShowWalkInView() 
        {
            if (RequiredNotComplete())
                return;

            PatientUser patient = new PatientUser(Firstname, Middlename, Lastname, (DateTime)DOB, int.Parse(StreetNo), Postcode);
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
                    Alert("Incorrect ID.", "Invalid Patient ID. Please speak to a receptionist.");
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

            PatientDBConverter.UpdateEmail(patientID, Email);
            // Change VM of AppointmentTypeView
            AppointmentTypeView = WalkInView;
            IsBookingVisible = true;
            BookingSubviewVisible = "Visible";
            PatientCaptureWidth = "0";
            MessengerInstance.Send<double>(patientID);
        }

        public void ShowPatientGrid() 
        {
            IsBookingVisible = false;
            PatientCaptureWidth = "*";
        }

        public void ShowHome()
        {
            MessengerInstance.Send<string>("DecideHomeView");
        }
    }
}
