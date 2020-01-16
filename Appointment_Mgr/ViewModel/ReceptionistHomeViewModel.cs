using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Mgr.ViewModel
{
    public class ReceptionistHomeViewModel : ViewModelBase
    {
        public string ScreenMessage { get; set; }

        public ReceptionistHomeViewModel()
        {
            if (IsInDesignMode)
            {
                ScreenMessage = "Placeholder";
            }
            else 
            {
                ScreenMessage = "Placeholder";
            }
        }
    }
}
