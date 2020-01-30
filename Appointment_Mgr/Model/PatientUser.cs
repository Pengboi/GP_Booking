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

namespace Appointment_Mgr.Model
{
    public class PatientUser
    {
        private int _patientNum;
        private string _firstname, _middlename, _lastname, _DOB, _Gender, _email, _streetNumber, _postcode;

        private static string LoadConnectionString(string id = "Patients") { return ConfigurationManager.ConnectionStrings[id].ConnectionString; }
        private SQLiteConnection StartConnection()
        {
            SQLiteConnection connection = new SQLiteConnection(LoadConnectionString());
            connection.Open();
            return connection;
        }

        
        //email for contact --> should not be used to identify Patient.
        public PatientUser(string firstname, string middlename, string lastname,
                           DateTime dob)
        {
            this._firstname = firstname.ToLower();  this._middlename = string.IsNullOrWhiteSpace(middlename) ? middlename : middlename.ToLower(); this._lastname = lastname.ToLower();
            this._DOB = dob.ToString("dd/MM/yyyy");
            Console.WriteLine("firstname: " + _firstname + " Middlename: " + _middlename + " Lastname: " + _lastname +  _DOB);
        }

        public string GetName() 
        {
            return this._firstname;
        }

        public bool RecordExists()
        {
            SQLiteConnection conn = StartConnection();
            // If middlename is not null, include in search
            string cmdString = $"SELECT COUNT(*) FROM Patient_Data WHERE Firstname = @fname AND" + (!string.IsNullOrWhiteSpace(this._middlename) ? " Middlename = @mname AND" : "") +
                                " Lastname = @lname AND DOB = @dob";

            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Prepare();

            cmd.Parameters.Add("@fname", DbType.String).Value = this._firstname;
            if (!string.IsNullOrWhiteSpace(this._middlename))
                cmd.Parameters.Add("@mname", DbType.String).Value = this._middlename;
            cmd.Parameters.Add("@lname", DbType.String).Value = this._lastname;
            cmd.Parameters.Add("@dob", DbType.String).Value = this._DOB;


            int recordFound = Convert.ToInt32(cmd.ExecuteScalar());

            if (recordFound == 1)
            {
                conn.Close();
                return true;
            }
            conn.Close();
            return false;
        }
    }
}
