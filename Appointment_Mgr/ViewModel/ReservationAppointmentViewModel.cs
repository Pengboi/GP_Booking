using Appointment_Mgr.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Mgr.ViewModel
{
    public class ReservationAppointmentViewModel : ViewModelBase
    {
        private string _requestedDoctor = "None", _requestedGender = "None";
        private DateTime _selectedDate = DateTime.Now.AddDays(1).Date;
        private DataTable _avaliableTimes;

        public string RequestedDoctor 
        {
            get { return _requestedDoctor; }
            set 
            {
                _requestedDoctor = value;
                RaisePropertyChanged("RequestedDoctor");
            }
        }
        public string RequestedGender 
        {
            get { return _requestedGender; }
            set 
            {
                _requestedGender = value;
                RaisePropertyChanged("RequestedGender");
            }
        }
        public DataTable AvaliableTimes
        {
            get { return _avaliableTimes; }
            set 
            {
                _avaliableTimes = value;
                RaisePropertyChanged("AvaliableTimes");
            }
        }
        public DateTime SelectedDate
        {
            get { return _selectedDate; }
            set 
            {
                _selectedDate = value.Date;
                RaisePropertyChanged("SelectedDate");
            }
        }

        public ReservationAppointmentViewModel() 
        {
            

            if (IsInDesignMode)
            {
                SelectedDate = DateTime.Parse("17/02/2000");
            }
            else 
            {
                if (DateTime.Today.Date.DayOfWeek == DayOfWeek.Friday)
                    SelectedDate = DateTime.Now.AddDays(3).Date;
                else if (DateTime.Today.Date.DayOfWeek == DayOfWeek.Saturday)
                    SelectedDate = DateTime.Now.AddDays(2).Date;
                else
                    SelectedDate = DateTime.Now.AddDays(1).Date;
            }
            Console.WriteLine("At VM level");
            Console.WriteLine(SelectedDate + " " + RequestedDoctor + " " + RequestedGender);
            AvaliableTimes = StaffDBConverter.GetAvaliableTimeslots(SelectedDate, RequestedDoctor, RequestedGender);
        }

        public void UpdateTimeslots() 
        {
            AvaliableTimes = StaffDBConverter.GetAvaliableTimeslots(SelectedDate, RequestedDoctor, RequestedGender);
        }


    }
}
