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
    public class BookAppointmentViewModel : MainViewModel
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
                RaisePropertyChanged("PatientCaptureWidth");
            }
        }
        public bool IsBookingVisible
        {
            get { return _isBookingVisible; }
            set
            {
                _isBookingVisible = value;
                RaisePropertyChanged("IsBookingVisible");
            }
        }
        public string BookingWidth
        {
            get { return _bookingWidth; }
            set
            {
                _bookingWidth = value;
                RaisePropertyChanged("BookingWidth");
            }
        }
        public string BookingSubviewVisible
        {
            get { return _bookingSubviewVisible; }
            set
            {
                _bookingSubviewVisible = value;
                RaisePropertyChanged("BookingSubviewVisible");
            }
        }

        private Dialog.IDialogBoxService _dialogService;
        public ICommand AlertCommand { get; private set; }

        

        //Dialog box definitions
        private void Alert(string title, string message)
        {
            var dialog = new AlertBoxViewModel(title, message);
            var result = _dialogService.OpenDialog(dialog);
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

        // maybe change to return a Patient Model Object if patient is 
        public bool VerifyPatientDetails(PatientUser p)
        {
            if (!p.RecordExists())
            {
                Alert("Patient Not Found", "Patient record could not be found. Please speak to a receptionist to register as a patient with the GP");
                return false;
            }
            return true;
        }

        public void ShowReservationView() 
        {
            if (RequiredNotComplete())
                return;

            PatientUser patient = new PatientUser(Firstname, Middlename, Lastname, (DateTime)DOB, int.Parse(StreetNo), Postcode);
            // If existing patient record is not found, return.
            if (!VerifyPatientDetails(patient))
                return;

            int patientID = PatientDBConverter.GetPatientID(patient);
            PatientDBConverter.UpdateEmail(patientID, Email);

            AppointmentTypeView = ReservationView;
            
            // Shows the booking view after patient details & desired reservation type verified
            // sends patient user details as message to view
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
            if (!VerifyPatientDetails(patient))
                return;

            int patientID = PatientDBConverter.GetPatientID(patient);
            AppointmentTypeView = WalkInView;
            IsBookingVisible = true;
            BookingSubviewVisible = "Visible";
            PatientCaptureWidth = "0";
        }

        public void ShowPatientGrid() 
        {
            IsBookingVisible = false;
            PatientCaptureWidth = "*";
        }

        public void ShowHome()
        {
            MessengerInstance.Send<string>("DecideHomeView");
            Cleanup();

        }

        public override void Cleanup()
        {
            Messenger.Default.Unregister(this);
            MessengerInstance.Unregister(this);
            base.Cleanup();
            ViewModelLocator.Cleanup();
        }
    }
}
