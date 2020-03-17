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
        private DataRow _timeslot;

        public string EstimatedTime 
        {
            get { return _estimatedTime; }
            set 
            {
                _estimatedTime = value;
                RaisePropertyChanged("EstimatedTime");
            }
        }
        public DataRow Timeslot{ get; set; }
        
        public WalkInAppointmentViewModel() 
        {
            Timeslot = AppointmentLogic.CalcWalkInTimeslot(StaffDBConverter.GetWalkInTimeslots());
            if (IsInDesignMode)
            {
                EstimatedTime = "2 Hours 45 Minutes";
            }
            else 
            {
                if (Timeslot == null) 
                {
                    EstimatedTime = "No Avaliable Time. Try booking a reservation.";
                    //insert disabe button feature.
                }
                else
                {
                    EstimatedTime = CalcWaitTime(Timeslot);
                }
            }
            
            //insert button command
        }

        public DataRow SelectTimeslot(DataTable dt) 
        {
            if (dt.Rows.Count <= 0) 
            {
                return null;
            }

            return dt.Rows[0];
        }

        public string CalcWaitTime(DataRow dr) 
        {
            TimeSpan timeslot = TimeSpan.Parse(dr[1].ToString());
            TimeSpan timeNow = DateTime.Now.TimeOfDay;

            TimeSpan timeDifference = timeslot - timeNow;

            string waitEstimation = "";

            if (timeDifference.Hours == 0)
                waitEstimation = timeDifference.Minutes.ToString() + " Minutes.";
            else
                waitEstimation = timeDifference.Hours.ToString() + " Hours. " + timeDifference.Minutes.ToString() + " Minutes.";

            return waitEstimation ;
        }
    }
}
