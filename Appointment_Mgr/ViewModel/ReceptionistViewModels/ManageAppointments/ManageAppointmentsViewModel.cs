﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Appointment_Mgr.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Appointment_Mgr.Dialog;

namespace Appointment_Mgr.ViewModel
{
    public class ManageAppointmentsViewModel : ViewModelBase
    {

        private DataTable _allAppointments = new DataTable();
        private DataTable _filteredAppointments = new DataTable();
        private int? _selectedIndex;
        private string _cancelledFilter = "False";
        private Visibility _cancellationVisibility = Visibility.Collapsed, _editOptionsVisibility = Visibility.Visible;
        private IDialogBoxService _dialogService;

        private void Success(string title, string message)
        {
            var dialog = new SuccessBoxViewModel(title, message);
            var result = _dialogService.OpenDialog(dialog);
        }

        private string CancellationReason(string title)
        {
            var dialog = new CancellationReasonBoxViewModel(title, "");
            var result = _dialogService.OpenDialog(dialog);
            if (result == null)
                result = "";
            return result.ToString();
        }

        public string CancelledFilter 
        {
            get { return _cancelledFilter; }
            set 
            {
                _cancelledFilter = value;
                if (_cancelledFilter == "False")
                {
                    CancellationVisibility = Visibility.Collapsed;
                    EditOptionsVisibility = Visibility.Visible;
                    InitialiseDataTable();
                }
                else 
                {
                    CancellationVisibility = Visibility.Visible;
                    EditOptionsVisibility = Visibility.Collapsed;
                    InitialiseDataTable();
                }
                RaisePropertyChanged(nameof(CancelledFilter));
            }
        }

        public Visibility CancellationVisibility
        {
            get { return _cancellationVisibility; }
            set 
            {
                _cancellationVisibility = value;
                RaisePropertyChanged(nameof(CancellationVisibility));
            }
        }

        // If cancelled appointments are being viewed, appointment edit options are collapsed.
        public Visibility EditOptionsVisibility 
        {
            get { return _editOptionsVisibility; }
            set 
            {
                _editOptionsVisibility = value;
                RaisePropertyChanged(nameof(EditOptionsVisibility));
            }
        }

        public DataTable AllAppointments 
        {
            get { return _allAppointments; }
            set 
            {
                _allAppointments = value;
                RaisePropertyChanged(nameof(AllAppointments));
            }
        }
        public DataTable FilteredAppointments
        {
            get { return _filteredAppointments; }
            set
            {
                _filteredAppointments = value;
                RaisePropertyChanged(nameof(FilteredAppointments));
            }
        }
        public int? SelectedIndex 
        {
            get { return _selectedIndex; }
            set 
            {
                _selectedIndex = value;
                RaisePropertyChanged(nameof(SelectedIndex));
            }
        }

        public RelayCommand CheckInPatientCommand { get; set; }
        public RelayCommand CancelAppointmentCommand { get; set; }

        public ManageAppointmentsViewModel() 
        {
            _dialogService = new DialogBoxService();

            // By default, non-cancelled appointments are shown.
            CancelledFilter = "False"; 
            CancellationVisibility = Visibility.Collapsed; EditOptionsVisibility = Visibility.Visible;
            InitialiseDataTable();

            MessengerInstance.Register<NotificationMessage>(this, FilterRecords);

            CheckInPatientCommand = new RelayCommand(CheckInPatient);
            CancelAppointmentCommand = new RelayCommand(CancelAppointment);
        }

        public void InitialiseDataTable() 
        {
            if (CancelledFilter == "True")
            {
                AllAppointments = PatientDBConverter.GetCancelledAppointments().Copy();
                CancellationVisibility = Visibility.Visible;
                EditOptionsVisibility = Visibility.Collapsed;
            }
            else 
            {
                AllAppointments = PatientDBConverter.GetBookedAppointments().Copy();
                CancellationVisibility = Visibility.Collapsed;
                EditOptionsVisibility = Visibility.Visible;
            }
            FilteredAppointments = AllAppointments.Copy();
        }

        private void CheckInPatient()
        {

            if (SelectedIndex == null) //shouldnt be able to happen but to prevent crash --> return.
                return;
            int index = int.Parse(SelectedIndex.ToString(), System.Globalization.CultureInfo.InvariantCulture);
            int appointmentID = int.Parse(FilteredAppointments.Rows[index][0].ToString(), System.Globalization.CultureInfo.InvariantCulture);
            PatientDBConverter.CheckInPatient(appointmentID);
            Success("Success!", "Patient has been checked-in.");
            InitialiseDataTable(); // To refresh table after adjustments --> triggering onpropertychange
        }
        private void CancelAppointment()
        {
            if (SelectedIndex == null) //shouldnt be able to happen but to prevent crash --> return.
                return;
            int index = int.Parse(SelectedIndex.ToString(), System.Globalization.CultureInfo.InvariantCulture);
            int appointmentID = int.Parse(FilteredAppointments.Rows[index][0].ToString(), System.Globalization.CultureInfo.InvariantCulture);

            string reason = CancellationReason("Reason For Appointment Cancellation?");
            if (reason == "")   // If user closes dialog box, operation is cancelled and appointment remains active
                return;

            PatientDBConverter.DeleteAppointment(appointmentID, reason);     // Appointment Moved To Cancelled Schema
            Success("Success!", "Patient appointment has been cancelled.");
            InitialiseDataTable(); // To refresh table after adjustments --> triggering onpropertychange
        }
        

        /*
         * A DataTable copy of all patient appointments for the day is created.
         * Every time the search filter is updated, the function is called with
         * the string content of the search filter. Every record is then compared
         * against the inputted string and if a match is not existent, the row is 
         * removed. Once all rows have been compared, the DataTable remaining
         * rows will be matches found from the inputted search string.
         * 
         * Everytime the function is called, the viewable DataTable is reset to a
         * copy of all todays appointments before being filtered against the inputted
         * string.
         */
        private void FilterRecords(NotificationMessage msg) 
        {
            string filterMessage = msg.Notification;
            filterMessage = filterMessage.ToLower(new System.Globalization.CultureInfo("en-UK", false));
            
            FilteredAppointments = AllAppointments.Copy();

            if (!string.IsNullOrWhiteSpace(filterMessage))
            {
                // For each match found, i's value is reduced by 1 to reflect the change in the viewable DataTable's
                // length. This ensures no row is skipped on each iteration of the comparison.
                for (int i = 0; i < FilteredAppointments.Rows.Count; i++) 
                {
                    var tempRow = FilteredAppointments.Rows[i];
                    string tempName = FilteredAppointments.Rows[i]["PatientName"].ToString();
                    tempName = tempName.ToLower(new System.Globalization.CultureInfo("en-UK", false));

                    if (tempName.Contains(filterMessage))
                    {
                        if (tempName.Length < filterMessage.Length)
                        {
                            FilteredAppointments.Rows.Remove(tempRow);
                            i -= 1;
                        }
                    }
                    else 
                    {
                        FilteredAppointments.Rows.Remove(tempRow);
                        i -= 1;
                    }
                }
            }
        }
    }
}
