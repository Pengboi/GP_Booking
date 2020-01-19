using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Mgr.ViewModel
{
    public class BookAppointmentViewModel : MainViewModel
    {
        public RelayCommand WalkInCommand { private set; get; }
        public RelayCommand ReservationCommand { private set; get; }

        public string Firstname { get; set; }
        public string Middlename { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }

        public BookAppointmentViewModel()
        {
            WalkInCommand = new RelayCommand(ShowWalkInView);
            ReservationCommand = new RelayCommand(ShowWalkInView);
        }

        public void ShowReservationView() 
        {
            ValidatePatientInput();
            VerifyPatientDetails();
        }
        public void ShowWalkInView() 
        {
            ValidatePatientInput();
            VerifyPatientDetails();
        }

        // To validate inputted data
        public bool ValidatePatientInput() 
        {

            return false;
        }
        // maybe change to return a Patient Model Object if patient is 
        public bool VerifyPatientDetails() 
        {


            return false;
        }
    }
}
