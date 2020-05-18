using Appointment_Mgr.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Appointment_Mgr.ViewModel
{
    public class DoctorAppointmentViewModel : ViewModelBase
    {
        public DataRow _activeAppointment = null;
        public string _patientName, _appointmentTime, _appointmentNotes;

        public int DoctorID { get; set; }

        public DataRow ActiveAppointment 
        {
            get { return _activeAppointment; }
            set 
            {
                _activeAppointment = value;
                RaisePropertyChanged(nameof(ActiveAppointment));
            }
        }

        public string PatientName
        {
            get { return _patientName; }
            set
            {
                _patientName = Regex.Replace(value, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
                RaisePropertyChanged(nameof(PatientName));
            }
        }

        public string AppointmentTime { 
            get { return _appointmentTime; }
            set 
            {
                _appointmentTime = value;
                RaisePropertyChanged(nameof(AppointmentTime));
            } 
        }

        public string AppointmentNotes 
        {
            get { return _appointmentNotes; }
            set 
            {
                if (string.IsNullOrEmpty(value))
                    value = "No Appointment Notes.";
                _appointmentNotes = value;
                RaisePropertyChanged(nameof(AppointmentNotes));
            }
        }

        public RelayCommand EndAppointmentCommand { get; set; }

        public DoctorAppointmentViewModel() 
        {
            if (IsInDesignMode) 
            {
                PatientName = "John Lee";
                AppointmentTime = "12:40";
            }
            MessengerInstance.Register<int>(this, SetDoctorDetails);

            EndAppointmentCommand = new RelayCommand(EndAppointment);
        }

        public void SetDoctorDetails(int msg)
        {
            DoctorID = msg;

            ActiveAppointment = PatientDBConverter.GetDoctorActiveAppointment(DoctorID);
            AppointmentTime = ActiveAppointment[2].ToString();
            PatientName = PatientDBConverter.GetPatientName(int.Parse(ActiveAppointment[3].ToString()));
            AppointmentNotes = ActiveAppointment[4].ToString();
        }

        public void EndAppointment()
        {
            TimeSpan timeNow = DateTime.Now.TimeOfDay;
            string startTimeString = ActiveAppointment[6].ToString();
            TimeSpan startTime = TimeSpan.Parse(startTimeString.Insert(startTimeString.Length - 2, ":"));

            int appointmentDuration = (int)(timeNow - startTime).TotalMinutes;
            AppointmentLogic.EndAppointment(ActiveAppointment, appointmentDuration);

            MessengerInstance.Send<string>("DoctorHomeView");
            return;
        }
    }
}
