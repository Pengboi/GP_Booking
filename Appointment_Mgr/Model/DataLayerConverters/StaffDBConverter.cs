﻿using System;
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
        private static string LoadConnectionString(string id) {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString.Replace("{AppDir}", AppDomain.CurrentDomain.BaseDirectory); 
        }
        public static SQLiteConnection OpenConnection(string id = "Staff")
        {
            SQLiteConnection connection = new SQLiteConnection(LoadConnectionString(id), true);
            connection.Open();
            return connection;
        }

        public static List<string> GetDoctorList()
        {
            List<string> doctorList = new List<string> { "None" };

            SQLiteConnection conn = OpenConnection();
            string cmdString = $"SELECT Suffix || ' ' || Firstname || COALESCE(' ' || Middlename, '') || ' ' || Lastname " +
                                "FROM Accounts " +
                                "WHERE Account_Type = 2";
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                doctorList.Add(reader[0].ToString());
            }
            reader.Close();
            cmd.Dispose();
            conn.Close();
            return doctorList;
        }

       

        public static DataTable GetAvaliableTimeslots(DateTime date, string doctor = "None", string gender = "None")
        {
            date = date.Date;
            DataTable workingDoctors = GetWorkingDoctors(date, doctor, gender);

            // Doctor shift start & end times stored to be used to calculate 
            List<int> staffID = new List<int>();
            List<int> shiftStarts = new List<int>();
            List<int> shiftEnds = new List<int>();
            foreach (DataRow row in workingDoctors.Rows)
            {
                staffID.Add(int.Parse(row["Id"].ToString()));

                shiftStarts.Add(int.Parse(row["Shift_Start"].ToString()));
                shiftEnds.Add(int.Parse(row["Shift_End"].ToString()));
            }

            DataTable dt = AppointmentLogic.CalcTimeslots(staffID, shiftStarts, shiftEnds, PatientDBConverter.GetBookedTimeslots(date, staffID), true);

            return dt;
        }

        /*
         *  Function Purpose: Return DataTable of doctors working on given day with optional filter parameters of doctor name and gender
         *  
         *  This function utalises SQL queries which uses Accounts & Schedule Tables FROM the Staff Database to retrieve the:
         *  Suffix, Firstname, Middlename (if exists), Lastname, Gender, Schdule start time & end time of doctors.
         *
         *  IF either optional field is fulfilled, those parameters are also included within the SQL Command arguement to filter results
         *  Return Format of DataTable:
         *  ID | Suffix | Firstname | Middlename | Lastname | Shift Start | Shift End  
         */

        // You might be able to shorten all of this if you dont need all above columns
        public static DataTable GetWorkingDoctors(DateTime date, string doctor = "None", string gender = "None")
        {
            SQLiteConnection conn = OpenConnection();
            string cmdString = "";

            // If no optional field defined
            if (doctor == "None" && gender == "None")
            {
                cmdString = $"SELECT Accounts.Id, Accounts.Suffix, Accounts.Firstname, Accounts.Middlename, Accounts.Lastname, Schedule.Shift_Start, Schedule.Shift_End " +
                             "FROM Accounts, Schedule " +
                             "WHERE Accounts.Username = Schedule.Username AND Schedule.Date = @Date";

                SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
                cmd.Prepare();
                cmd.Parameters.Add("@Date", DbType.String).Value = date.ToShortDateString();
                SQLiteDataAdapter sqlda = new SQLiteDataAdapter(cmd);
                DataTable dt = new DataTable("PatientData");
                sqlda.Fill(dt);
                conn.Close();
                return dt;
            }
            // If only doctor optional field defined
            else if (doctor != "None" && gender == "None")
            {
                doctor = doctor.Substring(4, doctor.Length - 4); // Removes suffix + leading whitespace from selected doctor name
                List<string> doctorNames = doctor.Split(' ').ToList();

                cmdString = $"SELECT Accounts.Id, Accounts.Suffix, Accounts.Firstname, Accounts.Middlename, Accounts.Lastname, Schedule.Shift_Start, Schedule.Shift_End "
                    + "FROM Accounts, Schedule WHERE Accounts.Username = Schedule.Username AND Schedule.Date = @Date AND Accounts.Firstname = @Firstname AND";
                if (doctorNames.Count == 3)
                    cmdString = cmdString + " Accounts.Middlename = @Middlename AND Accounts.Lastname = @Lastname";
                else
                    cmdString = cmdString + " Accounts.Lastname = @Lastname";

                SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
                cmd.Prepare();
                cmd.Parameters.Add("@Date", DbType.String).Value = date.ToShortDateString();
                cmd.Parameters.Add("@Firstname", DbType.String).Value = doctorNames[0];
                if (doctorNames.Count == 3)
                {
                    cmd.Parameters.Add("@Middlename", DbType.String).Value = doctorNames[1];
                    cmd.Parameters.Add("@Lastname", DbType.String).Value = doctorNames[2];
                }
                else
                    cmd.Parameters.Add("@Lastname", DbType.String).Value = doctorNames[1];
                SQLiteDataAdapter sqlda = new SQLiteDataAdapter(cmd);
                DataTable dt = new DataTable("PatientData");
                sqlda.Fill(dt);
                conn.Close();
                return dt;
            }
            // If only gender optional field defined
            else if (doctor == "None" && gender != "None")
            {
                cmdString = $"SELECT Accounts.Id, Accounts.Suffix, Accounts.Firstname, Accounts.Middlename, Accounts.Lastname, Schedule.Shift_Start, Schedule.Shift_End " +
                             "FROM Accounts, Schedule " +
                             "WHERE Accounts.Username = Schedule.Username AND Schedule.Date = @Date AND Accounts.Gender = @Gender";
                SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
                cmd.Prepare();
                cmd.Parameters.Add("@Date", DbType.String).Value = date.ToShortDateString();
                cmd.Parameters.Add("@Gender", DbType.String).Value = gender;
                SQLiteDataAdapter sqlda = new SQLiteDataAdapter(cmd);
                DataTable dt = new DataTable("PatientData");
                sqlda.Fill(dt);
                conn.Close();
                return dt;
            }
            // If both optional fields defined
            else
            {
                doctor = doctor.Substring(4, doctor.Length - 4); // Removes suffix + leading whitespace from selected doctor name
                List<string> doctorNames = doctor.Split(' ').ToList();
                cmdString = $"SELECT Accounts.Id, Accounts.Suffix, Accounts.Firstname, Accounts.Middlename, Accounts.Lastname, Schedule.Shift_Start, Schedule.Shift_End "
                    + "FROM Accounts, Schedule WHERE Accounts.Username = Schedule.Username AND Schedule.Date = @Date AND Accounts.Gender = @Gender AND Accounts.Firstname = @Firstname AND";
                if (doctorNames.Count == 3)
                    cmdString = cmdString + " Accounts.Middlename = @Middlename AND Accounts.Lastname = @Lastname";
                else
                    cmdString = cmdString + " Accounts.Lastname = @Lastname";
                SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
                cmd.Prepare();
                cmd.Parameters.Add("@Date", DbType.String).Value = date.ToShortDateString();
                cmd.Parameters.Add("@Firstname", DbType.String).Value = doctorNames[0];
                if (doctorNames.Count == 3)
                {
                    cmd.Parameters.Add("@Middlename", DbType.String).Value = doctorNames[1];
                    cmd.Parameters.Add("@Lastname", DbType.String).Value = doctorNames[2];
                }
                else
                    cmd.Parameters.Add("@Lastname", DbType.String).Value = doctorNames[1];
                cmd.Parameters.Add("@Gender", DbType.String).Value = gender;
                SQLiteDataAdapter sqlda = new SQLiteDataAdapter(cmd);
                DataTable dt = new DataTable("PatientData");
                sqlda.Fill(dt);
                conn.Close();
                return dt;
            }
        }

        public static int GetRemainingShiftInMinutes(int id) 
        {
            string cmdString = "SELECT Schedule.Shift_End FROM Schedule, Accounts " +
                "WHERE Schedule.Date = @dateToday AND Schedule.Username = Accounts.Username AND Accounts.Id = @id";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Parameters.Add("@dateToday", DbType.String).Value = DateTime.Today.ToShortDateString();
            cmd.Parameters.Add("@id", DbType.Int32).Value = id;

            object retrievedShiftEnd = cmd.ExecuteScalar();
            if (retrievedShiftEnd == null)             // If no shift is found, 0 minutes remaining is returned as result
                return 0;
            string shiftEndString = retrievedShiftEnd.ToString();
            shiftEndString = shiftEndString.Insert(shiftEndString.Length - 2, ":");

            cmd.Dispose();
            conn.Close();

            TimeSpan currentTime = DateTime.Today.TimeOfDay;
            TimeSpan shiftEnd = TimeSpan.Parse(shiftEndString);

            return (int)(shiftEnd - currentTime).TotalMinutes;
        }

        public static string GetEmployeeNameByID(int id) 
        {
            string employeeName = "";

            SQLiteConnection conn = OpenConnection();
            string cmdString = "SELECT Suffix, Firstname, Middlename, Lastname FROM Accounts WHERE Id = @id;";
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Parameters.Add("@id", DbType.Int32).Value = id;
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string middlename = "";
                if (!(reader["Middlename"] == null))
                    middlename = reader["Middlename"].ToString();

                employeeName = reader["Suffix"].ToString() + " " + reader["Firstname"].ToString() + " " + (!(string.IsNullOrWhiteSpace(middlename)) ? middlename + " " : "") + reader["Lastname"].ToString();
            }
            reader.Close();
            conn.Close();

            return employeeName;
        }

        public static int GetAccountIDByUsername(string username)
        {
            int doctorID = 0;
            SQLiteConnection conn = OpenConnection();
            string cmdString = "SELECT Id FROM Accounts WHERE Username = @username;";
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Parameters.Add("@username", DbType.String).Value = username;
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                doctorID = reader.GetInt32(0);
            }
            reader.Close();
            cmd.Dispose();
            conn.Close();

            return doctorID;
        }
    }
}
