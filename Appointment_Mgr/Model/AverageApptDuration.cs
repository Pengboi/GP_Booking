using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Data.SQLite;

namespace Appointment_Mgr.Model
{
    public static class AverageApptDuration
    {
        private static string path = ConfigurationManager.AppSettings.Get("AvgDurationFile");
        private static string LoadConnectionString(string id) { return ConfigurationManager.ConnectionStrings[id].ConnectionString; }
        private static SQLiteConnection OpenConnection(string id = "Patients")
        {
            SQLiteConnection connection = new SQLiteConnection(LoadConnectionString(id));
            connection.Open();
            return connection;
        }

        public static int averageDuration 
        {
            get { return getAverage(); }
            set { value = getAverage(); }
        }

        public static int getAverage()
        {
            int averageTime;

            //using path in app.config, read firstline of AvgApptDuration.txt (where duration is kept)
            using (StreamReader sr = new StreamReader(path)) 
            { averageTime = int.Parse(sr.ReadLine()); }

            return averageTime;
        }
        public static void setAverage() 
        {
            int calculatedAverage;

            SQLiteConnection conn = OpenConnection();
            string cmdString = @"SELECT AVG(Appointment_Duration) FROM Completed_Appointments";
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);

            calculatedAverage = int.Parse(cmd.ExecuteNonQuery().ToString());

            using (StreamWriter sw = new StreamWriter(path)) 
            { sw.Write(calculatedAverage);  }
        }
    }
}
