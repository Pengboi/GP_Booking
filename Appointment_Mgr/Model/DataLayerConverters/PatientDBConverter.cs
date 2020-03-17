using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Mgr.Model
{
    public class PatientDBConverter
    {
        private static string LoadConnectionString(string id) { return ConfigurationManager.ConnectionStrings[id].ConnectionString; }
        public static SQLiteConnection OpenConnection(string id = "Patients")
        {
            SQLiteConnection connection = new SQLiteConnection(LoadConnectionString(id));
            connection.Open();
            return connection;
        }

        public static int GetPatientID(PatientUser p) 
        {
            SQLiteConnection conn = OpenConnection();
            // If middlename is not null, include in search
            string cmdString = $"SELECT \"Patient#\" FROM Patient_Data WHERE Firstname = @fname AND" + (!string.IsNullOrWhiteSpace(p.Middlename) ? " Middlename = @mname AND" : "") +
                                " Lastname = @lname AND DOB = @dob AND ST_Number = @stNum AND Postcode = @postcode";

            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Prepare();

            cmd.Parameters.Add("@fname", DbType.String).Value = p.Firstname;
            if (!string.IsNullOrWhiteSpace(p.Middlename))
                cmd.Parameters.Add("@mname", DbType.String).Value = p.Middlename;
            cmd.Parameters.Add("@lname", DbType.String).Value = p.Lastname;
            cmd.Parameters.Add("@dob", DbType.String).Value = p.DOB.ToShortDateString();
            cmd.Parameters.Add("@stNum", DbType.Int32).Value = p.StreetNo;
            cmd.Parameters.Add("@postcode", DbType.String).Value = p.Postcode;

            //returns patient's ID number
            int pID = int.Parse(cmd.ExecuteScalar().ToString());
            return pID;
        }
        public static DataTable GetPatients()
        {
            //load from db into pList
            SQLiteConnection conn = OpenConnection();
            string cmdString = $"SELECT * FROM Patient_Data";
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            SQLiteDataAdapter sqlda = new SQLiteDataAdapter(cmd);
            DataTable dt = new DataTable("Patient_Data");
            sqlda.Fill(dt);
            conn.Close();

            return dt;
        }
        public static string GetEmail(int patientID) 
        {
            string cmdString = "SELECT \"E-mail\" FROM Patient_Data WHERE \"Patient#\" = @id";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Prepare();
            cmd.Parameters.Add("@id", DbType.Int32).Value = patientID;
            string email = cmd.ExecuteScalar().ToString();
            conn.Close();
            return email;
        }
        public static void UpdateEmail(int patientID, string email)
        {
            string cmdString = "UPDATE Patient_Data SET \"E-mail\" = @email WHERE \"Patient#\" = @id";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Prepare();
            cmd.Parameters.Add("@email", DbType.String).Value = email;
            cmd.Parameters.Add("@id", DbType.Int32).Value = patientID;
            cmd.ExecuteNonQuery();
            conn.Close();
            return;
        }

        public static void SaveChanges(DataTable dt) 
        {
            string cmdString = "SELECT * FROM Patient_Data";
            SQLiteConnection conn = OpenConnection();
            SQLiteDataAdapter sqlDa = new SQLiteDataAdapter();
            sqlDa.SelectCommand = new SQLiteCommand(cmdString, conn);
            sqlDa.Fill(dt);
            sqlDa.UpdateCommand = new SQLiteCommandBuilder(sqlDa).GetUpdateCommand();
            sqlDa.Update(dt);
            conn.Close();
        }

        public static void SaveRemoval(string index)
        {
            string cmdString = @"DELETE FROM Patient_Data WHERE Firstname in (SELECT Firstname FROM Patient_Data LIMIT 1 OFFSET " 
                + index + ")";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public static void BookAppointment(string timeslot, int doctorID, int patientID, string notes = null, string date = null) 
        {
            if (date == null)
                date = DateTime.Today.ToShortDateString();

            string cmdString = @"INSERT INTO Booked_Appointments (Date, Patient_ID, Assigned_Doctor_ID, Patient_Notes, Appointment_Time)
                                 VALUES (@date, @patientID, @doctorID, @patientNotes, @apptTime)";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);

            cmd.Parameters.Add("@date", DbType.String).Value = date;
            cmd.Parameters.Add("@patientID", DbType.Int32).Value = patientID;
            cmd.Parameters.Add("@doctorID", DbType.Int32).Value = doctorID;
            if (string.IsNullOrWhiteSpace(notes))
                cmd.Parameters.Add("@patientNotes", DbType.String).Value = DBNull.Value;
            else
                cmd.Parameters.Add("@patientNotes", DbType.String).Value = notes;
            cmd.Parameters.Add("@apptTime", DbType.Int32).Value = int.Parse(timeslot.Replace(":", ""));
            cmd.ExecuteNonQuery();
            conn.Close();
        }
    }
}
