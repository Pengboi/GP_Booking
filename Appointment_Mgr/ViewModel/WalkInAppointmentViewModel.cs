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

        public string EstimatedTime 
        {
            get { return _estimatedTime; }
            set 
            {
                _estimatedTime = value;
                RaisePropertyChanged("EstimatedTime");
            }
        }
        
        public WalkInAppointmentViewModel() 
        {
            if (IsInDesignMode)
            {
                EstimatedTime = "2 Hours 45 Minutes";
            }
            else 
            {
                EstimatedTime = "2 hours 10 mins";
            }
        }
    }
}
