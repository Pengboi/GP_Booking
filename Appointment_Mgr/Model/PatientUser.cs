using System;
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
        private string _firstname, _middlename, _lastname, _DOB, _gender, _email, _streetNumber, _postcode;

        private static string LoadConnectionString(string id = "Patients") { return ConfigurationManager.ConnectionStrings[id].ConnectionString; }
        private SQLiteConnection OpenConnection()
        {
            SQLiteConnection connection = new SQLiteConnection(LoadConnectionString());
            connection.Open();
            return connection;
        }


        //email for contact --> should not be used to identify Patient.
        public PatientUser(string firstname, string middlename, string lastname,
                           DateTime dob)
        {
            this._firstname = firstname.ToLower(); this._middlename = string.IsNullOrWhiteSpace(middlename) ? middlename : middlename.ToLower(); this._lastname = lastname.ToLower();
            this._DOB = dob.ToString("dd/MM/yyyy");
            Console.WriteLine("firstname: " + _firstname + " Middlename: " + _middlename + " Lastname: " + _lastname + _DOB);
        }

        public bool RecordExists()
        {
            SQLiteConnection conn = OpenConnection();
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

        public int GetPatientNo() { return _patientNum;  }
        public string GetFirstname() { return _firstname; }
        public string GetMiddlename() 
        {
            if (string.IsNullOrWhiteSpace(_middlename))
                return "";
            else
                return _middlename;
        }
        public string GetLastname() { return _lastname; }
        public string GetDOB() { return _DOB; }
        public string GetGender() { return _gender; }
        public string GetEmail() { return _email; }
        public string GetStreetNum() { return _streetNumber; }
        public string GetPostcode() { return _postcode; }

        public int PatientNo 
        {
            get { return _patientNum; }
            set { value = _patientNum; }
        }
        public string DOB 
        {
            get { return _DOB; }
            set { value = _DOB; }
        }
        public string Firstname
        {
            get { return _firstname; }
            set { value = _firstname; }
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
            }
        }
        public string Lastname 
        {
            get { return _lastname; }
            set { value = Lastname; }
        }
        public string Gender 
        {
            get { return _gender; }
            set { value = _gender; }
        }
        public string Email 
        {
            get { return _email; }
            set { value = _email; }
        }
        public string StreetNo 
        {
            get { return _streetNumber; }
            set { value = _email; }
        }
        public string Postcode 
        {
            get { return _postcode; }
            set { value = _postcode; }
        }

        /*  Below functions to be used by Model - Database Converter
         *  Used to set values for private variables defining patient model
         *  Which are stored in data layer. 
         */
        public void SetPatientNo(int number) { _patientNum = number; }
        public void SetGender(string gender) { _gender = gender; }
        public void SetEmail(string email) { _email = email; }
        public void SetStreetNum(string number) { _streetNumber = number; }
        public void SetPostcode(string postcode) { _postcode = postcode; }
    }
}
