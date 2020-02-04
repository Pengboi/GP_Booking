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
        private DateTime _selectedDate = DateTime.Now.Date;
        private DataTable _avaliableDoctors;

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
        public DataTable AvaliableDoctors 
        {
            get { return _avaliableDoctors; }
            set 
            {
                _avaliableDoctors = value;
                RaisePropertyChanged("AvaliableDoctors");
            }
        }
        public DateTime SelectedDate
        {
            get { return _selectedDate; }
            set 
            {
                value = SelectedDate;
                RaisePropertyChanged("SelectedDate");
            }
        }

        public ReservationAppointmentViewModel() 
        {
            SelectedDate = DateTime.Now.Date;
            if (IsInDesignMode)
            {
                //AvaliableDoctors = insert sample
            }
            else 
            {
                //AvaliableDoctors = StaffDBConverter.GetAvaliableDoctors(SelectedDate, RequestedDoctor, RequestedGender);
            }
        }


    }
}
