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
        public static SQLiteConnection OpenConnection(string id = "Staff")
        {
            SQLiteConnection connection = new SQLiteConnection(LoadConnectionString(id));
            connection.Open();
            return connection;
        }

        public static int GetPatientID(PatientUser p) 
        {
            SQLiteConnection conn = OpenConnection();
            // If middlename is not null, include in search
            string cmdString = $"SELECT Patient# FROM Patient_Data WHERE Firstname = @fname AND" + (!string.IsNullOrWhiteSpace(p.Middlename) ? " Middlename = @mname AND" : "") +
                                " Lastname = @lname AND DOB = @dob";

            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Prepare();

            cmd.Parameters.Add("@fname", DbType.String).Value = p.Firstname;
            if (!string.IsNullOrWhiteSpace(p.Middlename))
                cmd.Parameters.Add("@mname", DbType.String).Value = p.Middlename;
            cmd.Parameters.Add("@lname", DbType.String).Value = p.Lastname;
            cmd.Parameters.Add("@dob", DbType.String).Value = p.DOB;

            return 0;
        }
        public static DataTable GetPatients()
        {
            
            //load from db into pList
            SQLiteConnection conn = OpenConnection("Patients");
            string cmdString = $"SELECT * FROM Patient_Data";
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            SQLiteDataAdapter sqlda = new SQLiteDataAdapter(cmd);
            DataTable dt = new DataTable("Patient_Data");
            sqlda.Fill(dt);
            conn.Close();

            return dt;
        }

        public static void SaveChanges(DataTable dt) 
        {
            string cmdString = "SELECT * FROM Patient_Data";
            SQLiteConnection conn = OpenConnection("Patients");
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
            SQLiteConnection conn = OpenConnection("Patients");
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public static void BookAppointment(string timeslot, int doctorID, int patientID, string comment = null, string date = null) 
        {
            if (date == null)
                date = DateTime.Today.ToShortDateString();
        }
    }
}
