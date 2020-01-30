using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Mgr.ViewModel
{
    public class ManagePatientViewModel : ViewModelBase
    {
        private string _addIsSelected, _manageIsSelected, _deleteIsSelected;

        public string AddIsSelected
        {
            get { return this._addIsSelected; }
            set
            {
                this._addIsSelected = value;
                RaisePropertyChanged("AddIsSelected");
            }
        }
        public string ManageIsSelected
        {
            get { return this._manageIsSelected; }
            set
            {
                this._manageIsSelected = value;
                RaisePropertyChanged("ManageIsSelected");
            }
        }
        public string DeleteIsSelected
        {
            get { return this._deleteIsSelected; }
            set
            {
                this._deleteIsSelected = value;
                RaisePropertyChanged("DeleteIsSelected");
            }
        }

        public ViewModelBase CurrentViewModel { get; set; }

        public ViewModelBase AddPatientVM { get { return (ViewModelBase)ViewModelLocator.AddPatient; } }

        public ManagePatientViewModel() 
        {
            CurrentViewModel = AddPatientVM;
            
        }
    }
}
