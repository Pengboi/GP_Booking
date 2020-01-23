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

namespace Appointment_Mgr.ViewModel
{
    public class BookAppointmentViewModel : MainViewModel
    {
        public ViewModelBase _appointmentTypeViewModel;
        public ViewModelBase ReservationView { get { return (ViewModelBase)ViewModelLocator.ReservationAppointment; } }

        public RelayCommand WalkInCommand { private set; get; }
        public RelayCommand ReservationCommand { private set; get; }

        public string Firstname { get; set; }
        public string Middlename { get; set; }
        public string Lastname { get; set; }
        public DateTime? DOB { get; set; }
        public string Email { get; set; }
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
                RaisePropertyChanged(() => CurrentViewModel);
            }
        }

        public BookAppointmentViewModel()
        {
            _dialogService = new DialogBoxService();

            WalkInCommand = new RelayCommand(ShowWalkInView);
            ReservationCommand = new RelayCommand(ShowReservationView);
        }

        private bool RequiredNotComplete()
        {
            Console.WriteLine(Firstname); Console.WriteLine(Lastname); Console.WriteLine(DOB); Console.WriteLine(Email);
            if (string.IsNullOrWhiteSpace(Firstname)||
                string.IsNullOrWhiteSpace(Lastname) ||
                !DOB.HasValue                       ||
                string.IsNullOrWhiteSpace(Email)) 
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

            PatientUser patient = new PatientUser(Firstname, Middlename, Lastname, (DateTime)DOB);
            if (!VerifyPatientDetails(patient))
                return;
            AppointmentTypeView = ReservationView;
        }
        public void ShowWalkInView() 
        {
            if (RequiredNotComplete())
                return;

            PatientUser patient = new PatientUser(Firstname, Middlename, Lastname, (DateTime)DOB);
            if (!VerifyPatientDetails(patient))
                return;

        }

       
    }
}
