using Appointment_Mgr.Dialog;
using Appointment_Mgr.Dialog.Confirmation;
using Appointment_Mgr.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Appointment_Mgr.ViewModel
{
    public class WalkInAppointmentViewModel : ViewModelBase
    {
        private string _estimatedTime;
        private DataTable _timeslot;

        public string EstimatedTime 
        {
            get { return _estimatedTime; }
            set 
            {
                _estimatedTime = value;
                RaisePropertyChanged("EstimatedTime");
            }
        }
        public DataTable Timeslot{ get; set; }
        
        public WalkInAppointmentViewModel() 
        {
            Timeslot = StaffDBConverter.GetAvaliableTimeslots(DateTime.Today.Date);
            Console.WriteLine(timeslotToString(Timeslot));
            if (IsInDesignMode)
            {
                EstimatedTime = "2 Hours 45 Minutes";
            }
            else 
            {
                if (Timeslot.Rows.Count <= 0)
                    EstimatedTime = "No Avaliable Time. Try booking a reservation.";
                else
                {
                    //EstimatedTime = timeslotToString(Timeslot);
                }
            }
            
            //insert button command
        }

        public string timeslotToString(DataTable dt) 
        {
            return dt.Rows.ToString();
        }
    }
}
