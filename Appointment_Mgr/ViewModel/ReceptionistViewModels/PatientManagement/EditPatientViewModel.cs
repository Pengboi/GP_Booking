﻿using GalaSoft.MvvmLight;
using Appointment_Mgr.Model;
using System;
using System.Data;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using Appointment_Mgr.Dialog;
using System.Text.RegularExpressions;
using GalaSoft.MvvmLight.Messaging;

namespace Appointment_Mgr.ViewModel
{
    public class EditPatientViewModel : ViewModelBase
    {
        private IDialogBoxService _dialogService;
        public ICommand AlertCommand { get; private set; }
        public ICommand ErrorCommand { get; private set; }
        public ICommand SuccessCommand { get; private set; }

        private DataTable _patients = new DataTable();
        public RelayCommand SaveCommand { get; private set; }

        public DataTable Patients
        {
            get { return _patients; }
            set 
            { 
                _patients = value;
                RaisePropertyChanged(nameof(Patients));
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

        public void SaveToDB() 
        {
            //  Validation applies before changes are executted to Data Layer
            if (!EntryValidation())
                return;
            PatientDBConverter.SaveChanges(Patients);
            // Refreshes DataGrid view
            UpdateDB();

            Success("Database Updated.", "Changes to the database were successfully updated.");
        }

        private bool EntryValidation()
        {
            // For each row, in each column the following validation checks are performed:
            // If whitespace is contained in otherwise valid strings --> remove whitespace (assume unintended user error)
            // If value contains nothing but whitespace --> set value as null

            // If column is Postcode --> validate against postcode, if not a match, error presented and changes are prevented until issue is fixed
            foreach (DataRow dr in Patients.Rows) 
            {
                foreach (DataColumn col in Patients.Columns) 
                {
                    if (col.DataType == typeof(System.String)) 
                    {
                        dr[col] = dr[col].ToString().Replace(" ", "");
                        if (string.IsNullOrWhiteSpace(dr[col].ToString()))
                            dr[col] = DBNull.Value;
                    }
                    if (col.ColumnName == "Firstname") 
                    {
                        if (!Regex.IsMatch(dr[col].ToString(),  @"^[a-zA-Z]") || string.IsNullOrWhiteSpace(dr[col].ToString()))
                        {
                            Error("Changes not saved!", "An invalid Firstname has been entered. Changes have not been saved. Please resolve issue and try again.");
                            return false;
                        }
                    }
                    if (col.ColumnName == "Middlename")
                    {
                        if (!Regex.IsMatch(dr[col].ToString(), @"^[a-zA-Z]") && !string.IsNullOrWhiteSpace(dr[col].ToString()))
                        {
                            Error("Changes not saved!", "An invalid Middlename has been entered. Changes have not been saved. Please resolve issue and try again.");
                            return false;
                        }
                    }
                    if (col.ColumnName == "Lastname")
                    {
                        if (!Regex.IsMatch(dr[col].ToString(), @"^[a-zA-Z]") || string.IsNullOrWhiteSpace(dr[col].ToString()))
                        {
                            Error("Changes not saved!", "An invalid Lastname has been entered. Changes have not been saved. Please resolve issue and try again.");
                            return false;
                        }
                    }
                    if (col.ColumnName == "Postcode")
                    {
                        if (!Regex.IsMatch(dr[col].ToString(),
                            @"([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9][A-Za-z]?))))\s?[0-9][A-Za-z]{2})") ||
                            string.IsNullOrWhiteSpace(dr[col].ToString()))
                        {
                            Error("Changes not saved!", "An invalid postcode has been entered. Changes have not been saved. Please resolve issue and try again.");
                            return false;
                        }
                    }
                    if (col.ColumnName == "E-mail") 
                    {
                        if (!Regex.IsMatch(dr[col].ToString(), @"[\w-_.]*[@]{1}([\w]+[.][\w]+)+$") && !string.IsNullOrWhiteSpace(dr[col].ToString()))
                        {
                            Error("Changes not saved!", "An invalid email has been entered. Changes have not been saved. Please resolve issue and try again.");
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public EditPatientViewModel() 
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

            SaveCommand = new RelayCommand(SaveToDB);
        }

        private void UpdateDB()
        {
            Patients = PatientDBConverter.GetPatients();
        }
    }
}
