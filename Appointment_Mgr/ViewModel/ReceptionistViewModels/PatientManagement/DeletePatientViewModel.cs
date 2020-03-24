using GalaSoft.MvvmLight;
using Appointment_Mgr.Model;
using System;
using System.Data;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using Appointment_Mgr.Dialog;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Messaging;

namespace Appointment_Mgr.ViewModel
{
    public class DeletePatientViewModel : ViewModelBase
    {
        private IDialogBoxService _dialogService;
        public ICommand AlertCommand { get; private set; }
        public ICommand ErrorCommand { get; private set; }
        public ICommand SuccessCommand { get; private set; }

        private DataTable _patients = new DataTable();
        private int? _selectedRow = null;
        public RelayCommand DeleteCommand { get; private set; }

        public DataTable Patients
        {
            get { return _patients; }
            set
            {
                _patients = value;
                RaisePropertyChanged(nameof(Patients));
            }
        }

        public int? SelectedRow
        {
            get { return _selectedRow; }
            set 
            {
                _selectedRow = value;
                RaisePropertyChanged(nameof(SelectedRow));
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
            var dialog = new Dialog.ErrorBoxViewModel(title, message);
            var result = _dialogService.OpenDialog(dialog);
        }
        private void Success(string title, string message)
        {
            var dialog = new Dialog.SuccessBoxViewModel(title, message);
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
            int patientID = int.Parse(Patients.Rows[(int)SelectedRow][0].ToString(), System.Globalization.CultureInfo.InvariantCulture);
            PatientDBConverter.DeleteRecord(patientID);
            // Refreshes DataGrid view, Resets selected row to null
            SelectedRow = null;
            Success("Database Updated.", "Changes to the database were successfully updated.");
            UpdateDB();
        }

        public DeletePatientViewModel()
        {
            _dialogService = new DialogBoxService();

            Patients = PatientDBConverter.GetPatients();
            Messenger.Default.Register<NotificationMessage>(
                this,
                message =>
                {
                    UpdateDB();
                }
            );
            DeleteCommand = new RelayCommand(DeleteFromDB);

        }

        private void UpdateDB()
        {
            Patients = PatientDBConverter.GetPatients();
        }
    }
}
