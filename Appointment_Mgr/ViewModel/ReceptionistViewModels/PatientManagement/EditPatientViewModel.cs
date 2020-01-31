using GalaSoft.MvvmLight;
using Appointment_Mgr.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Appointment_Mgr.Model;

namespace Appointment_Mgr.ViewModel
{
    public class EditPatientViewModel : ViewModelBase
    {
        private ObservableCollection<PatientUser> _patients = new ObservableCollection<PatientUser>();

        public ObservableCollection<PatientUser> Patients
        {
            get { return _patients; }
            set { _patients = value; }
        }


        public EditPatientViewModel() 
        {
            if (IsInDesignMode)
            {
                //insert dummy data here
            }
            else 
            {
                Patients = PatientDBConverter.GetPatients();
                Console.WriteLine(Patients.ToString());
                
            }
        }
    }
}
