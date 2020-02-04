using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Mgr.ViewModel
{
    public class ReservationAppointmentViewModel : ViewModelBase
    {
        private DateTime _selectedDate = DateTime.Now.Date;

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
            
        }


    }
}
