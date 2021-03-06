﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Data;
using OtpNet;
using System.Windows;
using GalaSoft.MvvmLight;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Appointment_Mgr.Model
{
    public class PatientUser : ViewModelBase
    {
        private int _patientNum, _streetNumber;
        private string _firstname, _middlename, _lastname, _DOB, _gender, _email, _postcode;

        private static string LoadConnectionString(string id = "Patients") {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString.Replace("{AppDir}", AppDomain.CurrentDomain.BaseDirectory); 
        }
        private SQLiteConnection OpenConnection()
        {
            SQLiteConnection connection = new SQLiteConnection(LoadConnectionString(), true);
            connection.Open();
            return connection;
        }


        //email for contact --> should not be used to identify Patient.
        public PatientUser(string firstname, string middlename, string lastname,
                           DateTime dob, int streetNumber, string postcode)
        {
            this._firstname = firstname.ToLower(new System.Globalization.CultureInfo("en-UK", false)); this._middlename = string.IsNullOrWhiteSpace(middlename) ? middlename : middlename.ToLower(new System.Globalization.CultureInfo("en-UK", false)); this._lastname = lastname.ToLower(new System.Globalization.CultureInfo("en-UK", false));
            this._DOB = dob.ToString("dd/MM/yyyy");
            this._streetNumber = streetNumber;
            this._postcode = postcode.ToUpper().Replace(" ", "");
        }

        public int RecordsFound(int? optionalID = null)
        {
            SQLiteConnection conn = OpenConnection();
            string cmdString;
            // If middlename is not null, include in search, if ID of patient has been specified, include in search. 
            // (used for when multiple records have same attribute values, so primary key needs to be employed.)
            if (optionalID != null)
                cmdString = $"SELECT COUNT(*) FROM PatientData WHERE Firstname = @fname AND" + (!string.IsNullOrWhiteSpace(this._middlename) ? " Middlename = @mname AND" : "") +
                                 " Lastname = @lname AND DOB = @dob AND ST_Number = @stNum AND Postcode = @postcode AND PatientID = @id";
            else
                cmdString = $"SELECT COUNT(*) FROM PatientData WHERE Firstname = @fname AND" + (!string.IsNullOrWhiteSpace(this._middlename) ? " Middlename = @mname AND" : "") +
                                 " Lastname = @lname AND DOB = @dob AND ST_Number = @stNum AND Postcode = @postcode";

            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Prepare();
            cmd.Parameters.Add("@fname", DbType.String).Value = this._firstname;
            if (!string.IsNullOrWhiteSpace(this._middlename))
                cmd.Parameters.Add("@mname", DbType.String).Value = this._middlename;
            cmd.Parameters.Add("@lname", DbType.String).Value = this._lastname;
            cmd.Parameters.Add("@dob", DbType.String).Value = this._DOB;
            cmd.Parameters.Add("@stNum", DbType.Int32).Value = this._streetNumber;
            cmd.Parameters.Add("@postcode", DbType.String).Value = this._postcode;
            if (optionalID.HasValue)
                cmd.Parameters.Add("@id", DbType.Int32).Value = (int)optionalID;


            int recordsFound = Convert.ToInt32(cmd.ExecuteScalar());

            conn.Close();
            return recordsFound;
        }

        public int PatientNo 
        {
            get { return _patientNum; }
            set { value = _patientNum; }
        }
        public DateTime DOB 
        {
            get { return DateTime.Parse(_DOB); }
            set 
            { 
                value = DateTime.Parse(_DOB);
            }
        }
        public string Firstname
        {
            get { return _firstname; }
            set { value = _firstname; RaisePropertyChanged("Gender"); }
        }
        public string Middlename 
        {
            get { return _middlename; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    Middlename = "";
                else
                    value = _middlename;
                RaisePropertyChanged("Middlename");
            }
        }
        public string Lastname 
        {
            get { return _lastname; }
            set { value = Lastname; RaisePropertyChanged("Lastname"); }
        }
        public string Gender 
        {
            get { return _gender; }
            set { value = _gender; RaisePropertyChanged("Gender"); }
        }
        public string Email 
        {
            get { return _email; }
            set 
            { 
                value = _email;
                RaisePropertyChanged("Email");
            }
        }
        public int StreetNo 
        {
            get { return _streetNumber; }
            set { value = _streetNumber; RaisePropertyChanged("StreetNo"); }
        }
        public string Postcode 
        {
            get { return _postcode; }
            set { _postcode = value.ToUpper().Replace(" ", ""); RaisePropertyChanged("Postcode"); }
        }
        //Needed for checkboxes in Manage Patient view. (All of these are used for patient management tbh)
        public bool IsMale 
        {
            get { return true;  }
            set 
            {
                if (_gender == "Male")
                    value = true;
                else
                    value = false;
            }
        }
        public bool IsFemale
        {
            get { return true; }
            set
            {
                if (_gender == "Female")
                    value = true;
                else
                    value = false;
            }
        }

        /*  Below functions to be used by Model - Database Converter
         *  Used to set values for private variables defining patient model
         *  Which are stored in data layer. 
         */
        public void SetPatientNo(int number) { _patientNum = number; }
        public void SetGender(string gender) { _gender = gender; }
        public void SetEmail(string email) { _email = email; }
        public void SetStreetNum(int number) { _streetNumber = number; }
        public void SetPostcode(string postcode) { _postcode = postcode; }
    }
}
