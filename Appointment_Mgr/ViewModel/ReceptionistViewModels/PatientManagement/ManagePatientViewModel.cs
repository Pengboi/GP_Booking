using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Mgr.ViewModel
{
    public class ManagePatientViewModel : ViewModelBase
    {
        public ViewModelBase _currentViewModel;

        private string _addRecordTextColour, _editRecordTextColour, _deleteRecordTextColour;
        public RelayCommand AddPatientCommand { get; private set; }
        public RelayCommand EditPatientCommand { get; private set; }
        public RelayCommand DeletePatientCommand { get; private set; }

        public string AddRecordTextColour
        {
            get { return this._addRecordTextColour; }
            set
            {
                this._addRecordTextColour = value;
                RaisePropertyChanged("AddRecordTextColour");
            }
        }
        public string EditRecordTextColour
        {
            get { return this._editRecordTextColour; }
            set
            {
                this._editRecordTextColour = value;
                RaisePropertyChanged("EditRecordTextColour");
            }
        }
        public string DeleteRecordTextColour
        {
            get { return this._deleteRecordTextColour; }
            set
            {
                this._deleteRecordTextColour = value;
                RaisePropertyChanged("DeleteRecordTextColour");
            }
        }


        public ViewModelBase CurrentViewModel 
        {
            get  { return this._currentViewModel; }
            set 
            {
                this._currentViewModel = value;
                RaisePropertyChanged(() => CurrentViewModel);
            } 
        }
        public ViewModelBase AddPatientVM { get { return (ViewModelBase)ViewModelLocator.AddPatient; } }
        public ViewModelBase EditPatientVM { get { return (ViewModelBase)ViewModelLocator.EditPatient; } }
        public ViewModelBase DeletePatientVM { get { return (ViewModelBase)ViewModelLocator.DeletePatient; } }

        public void SetAddView() 
        {
            AddRecordTextColour = "#1e3799"; // Light blue hex code for selected navigation VM element
            EditRecordTextColour = "#2f3640"; // Dark Black for non-selected navigation VM element
            DeleteRecordTextColour = "#2f3640"; // Dark Black for non-selected navigation VM element
            CurrentViewModel = AddPatientVM;
        }
        public void SetEditView() 
        {
            AddRecordTextColour = "#2f3640"; 
            EditRecordTextColour = "#1e3799"; 
            DeleteRecordTextColour = "#2f3640";
            ViewModelLocator.Cleanup();
            CurrentViewModel = EditPatientVM;
        }
        public void SetDeleteView()
        {
            AddRecordTextColour = "#2f3640";
            EditRecordTextColour = "#2f3640";
            DeleteRecordTextColour = "#1e3799";
            ViewModelLocator.Cleanup();
            CurrentViewModel = DeletePatientVM;
        }

        public ManagePatientViewModel() 
        {
            CurrentViewModel = AddPatientVM;

            AddRecordTextColour = "#1e3799"; // Navy blue hex code for selected navigation VM element
            EditRecordTextColour = "#2f3640"; // Dark Black for non-selected navigation VM element
            DeleteRecordTextColour = "#000000"; // Dark Black for non-selected navigation VM element

            AddPatientCommand = new RelayCommand(SetAddView);
            EditPatientCommand = new RelayCommand(SetEditView);
            DeletePatientCommand = new RelayCommand(SetDeleteView);
        }

    }
}
