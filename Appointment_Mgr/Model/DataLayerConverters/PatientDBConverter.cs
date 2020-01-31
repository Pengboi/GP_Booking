using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
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

        public static ObservableCollection<PatientUser> GetPatients()
        {
            var pList = new ObservableCollection<PatientUser>();

            //load from db into pList
            SQLiteConnection conn = OpenConnection();
            string cmdString = $"SELECT * FROM Patient_Data";
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) 
            {
                PatientUser p = new PatientUser(reader["Firstname"].ToString(), reader["Middlename"].ToString(),
                    reader["Lastname"].ToString(), DateTime.Parse(reader["DOB"].ToString()));
                p.SetPatientNo(int.Parse(reader["Patient#"].ToString()));
                p.SetGender(reader["Gender"].ToString());
                p.SetEmail(reader["E-mail"].ToString());
                p.SetStreetNum(reader["ST_Number"].ToString());
                p.SetPostcode(reader["Postcode"].ToString());

                pList.Add(p);
            }
            Console.WriteLine(pList); // DEBUG
            reader.Close();
            conn.Close();
            return pList;
        }
    }
}
