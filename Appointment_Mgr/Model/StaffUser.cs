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

        private SQLiteConnection startConnection()
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
            SQLiteConnection conn = startConnection();
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
            SQLiteConnection conn = startConnection();
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

        public int getAccountType()
        {
            SQLiteConnection conn = startConnection();
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
            SQLiteConnection conn = startConnection();
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
        
    }
}
