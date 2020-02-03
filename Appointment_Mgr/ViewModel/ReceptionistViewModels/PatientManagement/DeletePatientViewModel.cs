using GalaSoft.MvvmLight;
using Appointment_Mgr.Model;
using System;
using System.Data;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using Appointment_Mgr.Dialog;
using System.Windows.Controls;

namespace Appointment_Mgr.ViewModel
{
    public class DeletePatientViewModel : ViewModelBase
    {
        private IDialogBoxService _dialogService;
        public ICommand AlertCommand { get; private set; }
        public ICommand ErrorCommand { get; private set; }
        public ICommand ConfirmationCommand { get; private set; }

        private DataTable _patients = new DataTable();
        private int? _selectedRow = null;
        public RelayCommand DeleteCommand { get; private set; }

        public DataTable Patients
        {
            get { return _patients; }
            set
            {
                _patients = value;
                RaisePropertyChanged("Patients");
            }
        }

        public int? SelectedRow
        {
            get { return _selectedRow; }
            set 
            {
                _selectedRow = value;
                RaisePropertyChanged("SelectedRow");
            }
        }

        //Dialog box definitions
        private void Alert(string title, string message)
        {
            var dialog = new AlertBoxViewModel(title, message);
            var result = _dialogService.OpenDialog(dialog);
        }
        private void Error(string title, string message)
        {
            var dialog = new Dialog.Error.ErrorBoxViewModel(title, message);
            var result = _dialogService.OpenDialog(dialog);
        }
        private void Confirmation(string title, string message)
        {
            var dialog = new Dialog.Confirmation.ConfirmationBoxViewModel(title, message);
            var result = _dialogService.OpenDialog(dialog);
        }

        public void DeleteFromDB()
        {
            // If no row was selected
            if (SelectedRow == null) 
            {
                Alert("No Row Selected!", "No record selected. Please select a record before attempting to delete a patient record.");
                return;
            }

            PatientDBConverter.SaveRemoval(SelectedRow.ToString());
            // Refreshes DataGrid view, Resets selected row to null
            Patients = null; SelectedRow = null;
            Patients = PatientDBConverter.GetPatients();
            Confirmation("Database Updated.", "Changes to the database were successfully updated.");
        }

        public DeletePatientViewModel()
        {
            _dialogService = new DialogBoxService();
            if (IsInDesignMode)
            {

            }
            else
            {
                Patients = PatientDBConverter.GetPatients();
            }
            DeleteCommand = new RelayCommand(DeleteFromDB);
        }
    }
}
