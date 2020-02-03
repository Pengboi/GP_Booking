﻿using GalaSoft.MvvmLight;
using Appointment_Mgr.Model;
using System;
using System.Data;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using Appointment_Mgr.Dialog;

namespace Appointment_Mgr.ViewModel
{
    public class EditPatientViewModel : ViewModelBase
    {
        private IDialogBoxService _dialogService;
        public ICommand AlertCommand { get; private set; }
        public ICommand ErrorCommand { get; private set; }
        public ICommand ConfirmationCommand { get; private set; }

        private DataTable _patients = new DataTable();
        public RelayCommand SaveCommand { get; private set; }

        public DataTable Patients
        {
            get { return _patients; }
            set 
            { 
                _patients = value;
                RaisePropertyChanged("Patients");
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

        public void SaveToDB() 
        {
            //EntryValidation(); // Add Validation laters
            PatientDBConverter.SaveChanges(Patients);
            // Refreshes DataGrid view
            Patients = null;
            Patients = PatientDBConverter.GetPatients();

            Confirmation("Database Updated.", "Changes to the database were successfully updated.");
        }

        private void EntryValidation()
        {
            // string.IsNullOrEmpty --> set as null.
            throw new NotImplementedException();
        }

        public EditPatientViewModel() 
        {
            _dialogService = new DialogBoxService();
            if (IsInDesignMode) 
            {

            }
            else 
            {
                Patients = PatientDBConverter.GetPatients();
            }
            SaveCommand = new RelayCommand(SaveToDB);
        }
    }
}
