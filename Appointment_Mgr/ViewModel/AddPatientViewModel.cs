using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Mgr.ViewModel
{
    public class AddPatientViewModel : ViewModelBase
    {

        public string Firstname { get; set; }
        public string Middlename { get; set; }
        public string Lastname { get; set; }

        public AddPatientViewModel() 
        {
        }
    }
}
