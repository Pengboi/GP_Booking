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
            Console.WriteLine(cmdString); //DEBUG
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }
    }
}
