using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Mgr.Model
{
    public class StaffDBConverter
    {
        private static string LoadConnectionString(string id) { return ConfigurationManager.ConnectionStrings[id].ConnectionString; }
        public static SQLiteConnection OpenConnection(string id = "Staff")
        {
            SQLiteConnection connection = new SQLiteConnection(LoadConnectionString(id));
            connection.Open();
            return connection;
        }

        /*
         *  Return Format of DataTable:
         *  Suffix | Firstname | Middlename | Lastname | Shift Start | Shift End  (shift start & end are stored in Military Time format)
         */
        public static DataTable GetAvaliableDoctors(DateTime date, string doctor, string gender)
        {
            SQLiteConnection conn = OpenConnection();
            string cmdString = "";

            /* 
             * This line contains an SQL query which uses Accounts & Schedule Tables FROM the Staff Database to retrieve the:
             * Suffix, Firstname, Middlename (if exists), Lastname, Gender, Schdule start time & end time of doctors.
             * IF either optional field is fulfilled, those parameters are also included within the SQL Command arguement.
             */
            
            // If no optional field defined
            if (doctor == "None" && gender == "None")
            {
                cmdString = $"SELECT Accounts.Suffix, Accounts.Firstname, Accounts.Middlename, Accounts.Lastname, Schedule.Shift_Start, Schedule.Shift_End " +
                             "FROM Accounts, Schedule " +
                             "WHERE Accounts.Username = Schedule.Username AND Schedule.Date = @Date";
                
                SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
                cmd.Prepare();
                cmd.Parameters.Add("@Date", DbType.String).Value = DateTime.UtcNow.Date.ToString("dd/MM/yyyy");
                SQLiteDataAdapter sqlda = new SQLiteDataAdapter(cmd);
                DataTable dt = new DataTable("Patient_Data");
                sqlda.Fill(dt);
                conn.Close();
                return dt;
            }
            // If only doctor optional field defined
            else if (doctor != "None" && gender == "None")
            {
                List<string> doctorNames = doctor.Split(' ').ToList();
                cmdString = $"SELECT Accounts.Suffix, Accounts.Firstname, Accounts.Middlename, Accounts.Lastname, Schedule.Shift_Start, Schedule.Shift_End "
                    + "FROM Accounts, Schedule WHERE Accounts.Username = Schedule.Username AND Schedule.Date = @Date AND Accounts.Firstname = @Firstname AND";
                if (doctorNames.Count == 3)
                    cmdString = cmdString + " Accounts.Middlename = @Middlename AND Accounts.Lastname = @Lastname";
                else
                    cmdString = cmdString + " Accounts.Lastname = @Lastname";
                
                SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
                cmd.Prepare();
                cmd.Parameters.Add("@Date", DbType.String).Value = DateTime.UtcNow.Date.ToString("dd/MM/yyyy");
                cmd.Parameters.Add("@Firstname", DbType.String).Value = doctorNames[0];
                if (doctorNames.Count == 3)
                {
                    cmd.Parameters.Add("@Middlename", DbType.String).Value = doctorNames[1];
                    cmd.Parameters.Add("@Lastname", DbType.String).Value = doctorNames[2];
                }
                else
                    cmd.Parameters.Add("@Lastname", DbType.String).Value = doctorNames[1];
                SQLiteDataAdapter sqlda = new SQLiteDataAdapter(cmd);
                DataTable dt = new DataTable("Patient_Data");
                sqlda.Fill(dt);
                conn.Close();
                return dt;
            }
            // If only gender optional field defined
            else if (doctor == "None" && gender != "None")
            {
                cmdString = $"SELECT Accounts.Suffix, Accounts.Firstname, Accounts.Middlename, Accounts.Lastname, Schedule.Shift_Start, Schedule.Shift_End " +
                             "FROM Accounts, Schedule " +
                             "WHERE Accounts.Username = Schedule.Username AND Schedule.Date = @Date AND Accounts.Gender = @Gender";
                SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
                cmd.Prepare();
                cmd.Parameters.Add("@Date", DbType.String).Value = DateTime.UtcNow.Date.ToString("dd/MM/yyyy");
                cmd.Parameters.Add("@Gender", DbType.String).Value = gender;
                SQLiteDataAdapter sqlda = new SQLiteDataAdapter(cmd);
                DataTable dt = new DataTable("Patient_Data");
                sqlda.Fill(dt);
                conn.Close();
                return dt;
            }
            // If both optional fields defined
            else
            {
                List<string> doctorNames = doctor.Split(' ').ToList();
                cmdString = $"SELECT Accounts.Suffix, Accounts.Firstname, Accounts.Middlename, Accounts.Lastname, Schedule.Shift_Start, Schedule.Shift_End "
                    + "FROM Accounts, Schedule WHERE Accounts.Username = Schedule.Username AND Schedule.Date = @Date AND Accounts.Firstname = @Firstname AND";
                if (doctorNames.Count == 3)
                    cmdString = cmdString + " Accounts.Middlename = @Middlename AND Accounts.Lastname = @Lastname";
                else
                    cmdString = cmdString + " Accounts.Lastname = @Lastname";
                cmdString = cmdString + " AND Accounts.Gender = @Gender";
                SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
                cmd.Prepare();
                cmd.Parameters.Add("@Date", DbType.String).Value = DateTime.UtcNow.Date.ToString("dd/MM/yyyy");
                if (doctorNames.Count == 3)
                {
                    cmd.Parameters.Add("@Middlename", DbType.String).Value = doctorNames[1];
                    cmd.Parameters.Add("@Lastname", DbType.String).Value = doctorNames[2];
                }
                else
                    cmd.Parameters.Add("@Lastname", DbType.String).Value = doctorNames[1];
                cmd.Parameters.Add("@Gender", DbType.String).Value = gender;
                SQLiteDataAdapter sqlda = new SQLiteDataAdapter(cmd);
                DataTable dt = new DataTable("Patient_Data");
                sqlda.Fill(dt);
                conn.Close();
                return dt;
            }
        }

    }
}
