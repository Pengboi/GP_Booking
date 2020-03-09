﻿using Appointment_Mgr.Dialog;
using Appointment_Mgr.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Appointment_Mgr.ViewModel
{
    public class ReservationAppointmentViewModel : ViewModelBase
    {
        private List<string> _genders = new List<string> { "None", "Male", "Female" }, _doctors = StaffDBConverter.GetDoctorList();
        private string _requestedDoctor = "None", _requestedGender = "None", _noAvaliableDate = "", _seeThis;
        private int? _selectedTimeslot = null;
        private DateTime _selectedDate = DateTime.Now.AddDays(1).Date;
        private DataTable _avaliableTimes;

        private IDialogBoxService _dialogService;

        public ICommand AlertCommand { get; private set; }
        public ICommand ErrorCommand { get; private set; }
        public ICommand OtpCommand { get; private set; }

        public List<string> Genders { get { return _genders; } }
        public List<string> Doctors { get { return _doctors; } }

        private string Otp()
        {
            var dialog = new Dialog.OTP.OTPBoxViewModel("", "Input your OTP code below:");
            var result = _dialogService.OpenDialog(dialog);
            return result;
        }
        private void Error()
        {
            throw new NotImplementedException();
        }
        private void Alert(string title, string message)
        {
            var dialog = new AlertBoxViewModel(title, message);
            var result = _dialogService.OpenDialog(dialog);
        }
        public string RequestedDoctor
        {
            get { return _requestedDoctor; }
            set
            {
                _requestedDoctor = value;
                RaisePropertyChanged("RequestedDoctor");
            }
        }
        public string RequestedGender
        {
            get { return _requestedGender; }
            set
            {
                _requestedGender = value;
                RaisePropertyChanged("RequestedGender");
            }
        }
        public DataTable AvaliableTimes
        {
            get { return _avaliableTimes; }
            set
            {
                _avaliableTimes = value;
                RaisePropertyChanged("AvaliableTimes");
            }
        }
        public string NoAvaliableTime
        {
            get { return _noAvaliableDate; }
            set
            {
                _noAvaliableDate = value;
                RaisePropertyChanged("NoAvaliableTime");
            }
        }
        public DateTime SelectedDate
        {
            get { return _selectedDate; }
            set
            {
                _selectedDate = value.Date;
                RaisePropertyChanged("SelectedDate");
            }
        }
        public int? TimeslotIndex
        {
            get { return _selectedTimeslot; }
            set
            {
                _selectedTimeslot = (int)value;
                RaisePropertyChanged("SelectedTimeslot");
            }
        }

        public RelayCommand BookAppointmentCommand { get; set; }

        public ReservationAppointmentViewModel()
        {
            _dialogService = new DialogBoxService();
            if (IsInDesignMode)
            {
                SelectedDate = DateTime.Parse("17/02/2000");
            }
            else
            {
                if (DateTime.Today.Date.DayOfWeek == DayOfWeek.Friday)
                    SelectedDate = DateTime.Now.AddDays(3).Date;
                else if (DateTime.Today.Date.DayOfWeek == DayOfWeek.Saturday)
                    SelectedDate = DateTime.Now.AddDays(2).Date;
                else
                    SelectedDate = DateTime.Now.AddDays(1).Date;
            }
            Console.WriteLine("INITIAL DATE: " + SelectedDate + " " + RequestedDoctor + " " + RequestedGender);
            AvaliableTimes = StaffDBConverter.GetAvaliableTimeslots(SelectedDate, RequestedDoctor, RequestedGender);
            if (AvaliableTimes.Rows.Count <= 0)
                NoAvaliableTime = "No Avaliable Times.";
            else
                NoAvaliableTime = "";

            Messenger.Default.Register<DateTime>
                (
                    this,
                    (action) => UpdateTimeslots()
                );
            Messenger.Default.Register<int>(this, UpdateTimeslotIndex);
            BookAppointmentCommand = new RelayCommand(BookAppointment);
        }

        public void UpdateTimeslots()
        {
            AvaliableTimes = StaffDBConverter.GetAvaliableTimeslots(SelectedDate, RequestedDoctor, RequestedGender);
            if (AvaliableTimes.Rows.Count <= 0)
                NoAvaliableTime = "No Avaliable Times.";
            else
                NoAvaliableTime = "";
        }
        private void UpdateTimeslotIndex(int index) { TimeslotIndex = index; }

        public void BookAppointment()
        {
            if (TimeslotIndex.Equals(-1))
            {
                Alert("No timeslot selected.", "Please select a timeslot for your appointment");
                return;
            }
            string selectedTimeslot = AvaliableTimes.Rows[(int)TimeslotIndex][1].ToString();
            string reservationDoctorID = AvaliableTimes.Rows[(int)TimeslotIndex][0].ToString();

            Alert(selectedTimeslot, reservationDoctorID);


        }
    }
}