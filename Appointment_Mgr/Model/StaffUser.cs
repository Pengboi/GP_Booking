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
    public class StaffUser
    {
        private string _username, _password, _otpToken;
        private int _accountType;

        // To be implemented later --> merge with other string instance variables upon implemntation (Or don't, keep as the "optional instance variable list"), idk.
        private string _suffix, _firstname, _middlename, _lastname, _gender;

        private SQLiteConnection OpenConnection()
        {
            SQLiteConnection connection = new SQLiteConnection(LoadConnectionString());
            connection.Open();
            return connection;
        }
        private static string LoadConnectionString(string id = "Default") { return ConfigurationManager.ConnectionStrings[id].ConnectionString; }

        public StaffUser(string username, string password)
        {
            this._username = username;
            this._password = password;

        }

        public bool userExists() 
        {
            SQLiteConnection conn = OpenConnection();
            string cmdString = $"SELECT COUNT(*) FROM Accounts WHERE Username = @uname";
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Prepare();
            cmd.Parameters.Add("@uname", DbType.String).Value = this._username;

            int recordsFound = Convert.ToInt32(cmd.ExecuteScalar());

            if (recordsFound == 1) 
            {
                conn.Close();
                return true;
            }
            conn.Close();
            return false;
        }

        public bool verifyPassword() 
        {
            if (string.IsNullOrWhiteSpace(this._password))
                return false;
            SQLiteConnection conn = OpenConnection();
            string cmdString = $"SELECT Password FROM Accounts WHERE Username = @uname";
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Prepare();
            cmd.Parameters.Add("@uname", DbType.String).Value = this._username;
            SQLiteDataReader reader = cmd.ExecuteReader();

            while (reader.Read()) 
            {
                if (BCrypt.Net.BCrypt.Verify(_password, reader["Password"].ToString())) 
                {
                    conn.Close();
                    return true;
                }
            }
            conn.Close();
            return false;
        }

        public string getUsername()
        {
            return this._username;
        }

        public string getSuffix() 
        {
            SQLiteConnection conn = OpenConnection();
            string cmdString = $"SELECT Suffix FROM Accounts WHERE Username = @uname";
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Prepare();
            cmd.Parameters.Add("@uname", DbType.String).Value = this._username;
            this._suffix = cmd.ExecuteScalar().ToString();
            conn.Close();
            return this._suffix;
        }
        public string getFirstname() 
        {
            SQLiteConnection conn = OpenConnection();
            string cmdString = $"SELECT Firstname FROM Accounts WHERE Username = @uname";
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Prepare();
            cmd.Parameters.Add("@uname", DbType.String).Value = this._username;
            this._firstname = cmd.ExecuteScalar().ToString();
            conn.Close();
            return this._firstname;
        }

        public string getMiddlename() 
        {
            string middlename = "";

            SQLiteConnection conn = OpenConnection();
            string cmdString = $"SELECT Middlename FROM Accounts WHERE Username = @uname";
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Prepare();
            cmd.Parameters.Add("@uname", DbType.String).Value = this._username;
            if (string.IsNullOrWhiteSpace(cmd.ExecuteScalar().ToString()))
                return "";

            return cmd.ExecuteScalar().ToString();
        }

        public string getLastname() 
        {
            SQLiteConnection conn = OpenConnection();
            string cmdString = $"SELECT Lastname FROM Accounts WHERE Username = @uname";
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Prepare();
            cmd.Parameters.Add("@uname", DbType.String).Value = this._username;
            string lastname = cmd.ExecuteScalar().ToString();
            conn.Close();
            return lastname;
        }

        public string getFullname() 
        {
            //TO BE CHANGED TO IMPLEMENT MIDDLENAME
            return getSuffix() + " " + getFirstname() + " " + getLastname();
        }

        public string getGender() 
        {
            SQLiteConnection conn = OpenConnection();
            string cmdString = $"SELECT Gender FROM Accounts WHERE Username = @uname";
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Prepare();
            cmd.Parameters.Add("@uname", DbType.String).Value = this._username;
            string gender = cmd.ExecuteScalar().ToString();
            conn.Close();
            return gender;
        }

        public int getAccountType()
        {
            SQLiteConnection conn = OpenConnection();
            string cmdString = $"SELECT Account_Type FROM Accounts WHERE Username = @uname";
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Prepare();
            cmd.Parameters.Add("@uname", DbType.String).Value = this._username;
            SQLiteDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            { this._accountType = reader.GetInt32(reader.GetOrdinal("Account_Type")); }

            conn.Close();
            return this._accountType;
        }

        public string getOTP() 
        {
            SQLiteConnection conn = OpenConnection();
            string cmdString = $"SELECT OTP_TOKEN FROM Accounts WHERE Username = @uname";
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Prepare();
            cmd.Parameters.Add("@uname", DbType.String).Value = this._username;
            this._otpToken = cmd.ExecuteScalar().ToString();
            conn.Close();

            return this._otpToken;
        }
        public bool verifyOTP(string inputOTP) 
        {
            this._otpToken = getOTP();
            var bytes = Base32Encoding.ToBytes(this._otpToken);
            var totp = new Totp(bytes);
            var totpCode = totp.ComputeTotp();

            if (inputOTP == totpCode)
                return true;
            else
                return false;
        }

        public bool isReceptionist()
        {
            if (getAccountType() == 1)
                return true;
            else
                return false;
        }
        public bool isDoctor() 
        {
            if (getAccountType() == 2)
                return true;
            else
                return false;
        }

        public List<StaffUser> GetAllDoctorNames() 
        {
            List<StaffUser> doctorList = new List<StaffUser>();

            SQLiteConnection conn = OpenConnection();
            string cmdString = $"SELECT Username FROM Accounts WHERE Account_Type = @type";
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Prepare();
            cmd.Parameters.Add("@tpye", DbType.Int32).Value = 2;

            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) 
            {
                doctorList.Add(new StaffUser(reader["Username"].ToString(), ""));
            }

            return doctorList;
        }
        
    }
}
