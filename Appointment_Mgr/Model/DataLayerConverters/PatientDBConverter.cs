using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Appointment_Mgr.Model
{
    public class PatientDBConverter
    {
        private static string LoadConnectionString(string id) {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString.Replace("{AppDir}", AppDomain.CurrentDomain.BaseDirectory); 
        }
        internal static SQLiteConnection OpenConnection(string id = "Patients")
        {
            SQLiteConnection connection = new SQLiteConnection(LoadConnectionString(id), true);
            connection.Open();
            return connection;
        }

        internal static int GetPatientID(PatientUser p) 
        {
            SQLiteConnection conn = OpenConnection();
            // If middlename is not null, include in search
            string cmdString = $"SELECT \"PatientID\" FROM PatientData WHERE Firstname = @fname AND" + (!string.IsNullOrWhiteSpace(p.Middlename) ? " Middlename = @mname AND" : "") +
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
            conn.Close();
            cmd.Dispose();
            return pID;
        }

        internal static string GetPatientName(int ID) 
        {
            string cmdString = "SELECT Firstname || COALESCE(' ' || Middlename, '') || ' ' || Lastname AS PatientName FROM PatientData WHERE PatientID = @id ";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Prepare();
            cmd.Parameters.Add("@id", DbType.Int32).Value = ID;

            string patientName = cmd.ExecuteScalar().ToString();
            conn.Close();
            cmd.Dispose();
            return patientName;

        }

        internal static DataTable GetPatients()
        {
            //load from db into pList
            SQLiteConnection conn = OpenConnection();
            string cmdString = $"SELECT * FROM PatientData";
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            SQLiteDataAdapter sqlda = new SQLiteDataAdapter(cmd);
            DataTable dt = new DataTable("PatientData");
            sqlda.Fill(dt);
            conn.Close();
            cmd.Dispose();
            sqlda.Dispose();

            return dt;
        }

        internal static string GetEmail(int patientID) 
        {
            string cmdString = "SELECT \"E-mail\" FROM PatientData WHERE \"PatientID\" = @id";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Prepare();
            cmd.Parameters.Add("@id", DbType.Int32).Value = patientID;
            string email = cmd.ExecuteScalar().ToString();
            conn.Close();
            return email;
        }
        internal static void UpdateEmail(int patientID, string email)
        {
            string cmdString = "UPDATE PatientData SET \"E-mail\" = @email WHERE \"PatientID\" = @id";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Prepare();
            cmd.Parameters.Add("@email", DbType.String).Value = email;
            cmd.Parameters.Add("@id", DbType.Int32).Value = patientID;
            cmd.ExecuteNonQuery();
            conn.Close();
            return;
        }

        internal static void SaveChanges(DataTable dt) 
        {
            string cmdString = "SELECT * FROM PatientData";
            SQLiteConnection conn = OpenConnection();
            SQLiteDataAdapter sqlDa = new SQLiteDataAdapter();
            sqlDa.SelectCommand = new SQLiteCommand(cmdString, conn);
            sqlDa.Fill(dt);
            sqlDa.UpdateCommand = new SQLiteCommandBuilder(sqlDa).GetUpdateCommand();
            sqlDa.Update(dt);
            conn.Close();
        }

        internal static void DeleteRecord(int id)
        {
            string cmdString = @"DELETE FROM PatientData WHERE PatientID = @id";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Parameters.Add("@id", DbType.Int32).Value = id;
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        internal static bool PatientHasAppointment(int patientID, string date = null) 
        {
            if (date == null)
                date = DateTime.Today.ToShortDateString();


            string cmdString = "SELECT COUNT(*) FROM BookedAppointments WHERE PatientID = @id AND Date = @date";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Parameters.Add("@id", DbType.Int32).Value = patientID;
            cmd.Parameters.Add("@date", DbType.String).Value = date;
            int recordExists = int.Parse(cmd.ExecuteScalar().ToString());
            if (recordExists > 0)
                return true;
            else
                return false;
        }

        internal static void BookAppointment(string timeslot, int doctorID, int patientID, bool isReservation, string notes = null, string date = null) 
        {
            if (date == null)
                date = DateTime.Today.ToShortDateString();

            string cmdString = @"INSERT INTO BookedAppointments (Date, PatientID, Assigned_DoctorID, Reservation, Patient_Notes, Appointment_Time)
                                 VALUES (@date, @patientID, @doctorID, @isReservation, @patientNotes, @apptTime)";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);

            cmd.Parameters.Add("@date", DbType.String).Value = date;
            cmd.Parameters.Add("@patientID", DbType.Int32).Value = patientID;
            cmd.Parameters.Add("@doctorID", DbType.Int32).Value = doctorID;
            if (isReservation)
                cmd.Parameters.Add("@isReservation", DbType.String).Value = "YES";
            else
                cmd.Parameters.Add("@isReservation", DbType.String).Value = "NO";
            if (string.IsNullOrWhiteSpace(notes))
                cmd.Parameters.Add("@patientNotes", DbType.String).Value = DBNull.Value;
            else
                cmd.Parameters.Add("@patientNotes", DbType.String).Value = notes;
            cmd.Parameters.Add("@apptTime", DbType.String).Value = timeslot.Replace(":", "");
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            conn.Close();
        }

        internal static void BookEmergencyAppointment(int patientID) 
        {
            string date = DateTime.Today.ToShortDateString();
            string appointmentTime = DateTime.Now.TimeOfDay.ToString("hhmm", System.Globalization.CultureInfo.GetCultureInfo("en-UK")).Replace(":", "");
            string cmdString = "INSERT INTO BookedAppointments(Date, PatientID, Reservation, Appointment_Time, Checked_In, Check_in_Time, Not_Emergency) " +
                "VALUES (@date, @patientID, \"NO\", @apptTime, @checkedIn, @checkInTime, @overridable)";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);

            cmd.Parameters.Add("@date", DbType.String).Value = date;
            cmd.Parameters.Add("@patientID", DbType.Int32).Value = patientID;
            cmd.Parameters.Add("@apptTime", DbType.String).Value = appointmentTime;
            cmd.Parameters.Add("@checkedIn", DbType.String).Value = "YES";
            cmd.Parameters.Add("@checkInTime", DbType.String).Value = appointmentTime;
            cmd.Parameters.Add("@overridable", DbType.String).Value = "NO";
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            conn.Close();
        }

        internal static void DeleteAppointment(int appointmentID, string reason = "Tardy") 
        {
            string cmdString = "INSERT INTO CancelledAppointments (AppointmentID, Assigned_DoctorID, Date, " +
                               "PatientID, Patient_Notes, Time, Reason ) "+
                               "SELECT AppointmentID, Assigned_DoctorID, Date, PatientID, Patient_Notes, Appointment_Time, @reason " +
                               "FROM BookedAppointments " +
                               "WHERE BookedAppointments.AppointmentID = @id; " +
                               "DELETE FROM BookedAppointments WHERE AppointmentID = @id;";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Parameters.Add("@reason", DbType.String).Value = reason;
            cmd.Parameters.Add("@id", DbType.Int32).Value = appointmentID;
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        // Respondsible for returning all booked appointments for all working doctors on any given working day.
        internal static List<List<int>> GetBookedTimeslots(DateTime date, List<int> ids)
        {
            List<List<int>> bookedTimeslots = new List<List<int>>();

            SQLiteConnection conn = OpenConnection("Patients");

            foreach (int id in ids)
            {
                List<int> selectedDoctorTimeslots = new List<int>();

                string cmdString = @"SELECT Appointment_Time " +
                                     "FROM BookedAppointments WHERE Date = @date AND Assigned_DoctorID = @id ";
                SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
                cmd.Prepare();
                cmd.Parameters.Add("@Date", DbType.String).Value = date.ToShortDateString();
                cmd.Parameters.Add("@id", DbType.String).Value = id;
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    selectedDoctorTimeslots.Add(int.Parse(reader.GetString(reader.GetOrdinal("Appointment_Time"))));
                }
                reader.Close();
                bookedTimeslots.Add(selectedDoctorTimeslots);
            }
            conn.Close();
            return bookedTimeslots;
        }

        internal static DataTable GetAwaitingCheckIn(DateTime dob, string firstname, string doorNo, string postcode) 
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("AppointmentID");
            dt.Columns.Add("AppointmentTime");
            dt.Columns.Add("DoctorName");
            dt.Columns.Add("PatientID");

            // The command begins by retrieving primary key Appointment number, Appointment time, doctor ID and patient Name which is then placed in the above datatable
            // within the specified columns - ID is temporarily placed in the DoctorName column. Only patients who have not been marked as Checked-in are retrieved.
            string cmdString = "SELECT DISTINCT BookedAppointments.AppointmentID, BookedAppointments.Appointment_Time, BookedAppointments.Assigned_DoctorID AS DoctorID, " +
                               "BookedAppointments.PatientID FROM BookedAppointments, PatientData " +
                               "WHERE BookedAppointments.Date = @date AND BookedAppointments.Checked_In = \"NO\" " +
                               "AND BookedAppointments.PatientID =  " +
                                    "(SELECT PatientData.PatientID FROM PatientData WHERE PatientData.Firstname = @firstname AND PatientData.ST_Number = @drNum " +
                                    "AND PatientData.Postcode = @postcode AND PatientData.DOB = @dob)";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);

            cmd.Parameters.Add("@date", DbType.String).Value = DateTime.Today.ToShortDateString();
            cmd.Parameters.Add("@firstname", DbType.String).Value = firstname.ToLower(new System.Globalization.CultureInfo("en-UK", false));
            cmd.Parameters.Add("@drNum", DbType.Int32).Value = doorNo;
            cmd.Parameters.Add("@postcode", DbType.String).Value = postcode.ToUpper().Replace(" ", "");
            cmd.Parameters.Add("@dob", DbType.String).Value = dob.ToShortDateString();

            SQLiteDataReader reader = cmd.ExecuteReader();

            while (reader.Read()) 
            {
                DataRow dr = dt.NewRow();
                dr[0] = reader.GetInt32(0);
                dr[1] = reader.GetString(1);
                dr[2] = reader.GetInt32(2);
                dr[3] = reader.GetInt32(3);
                dt.Rows.Add(dr);
            }
            reader.Close();
            conn.Close();
            
            // Using the obtained DoctorID, the DataTable is updated with the doctor Name using GetEmployeeNameByID - taking the previously obtained ID's as arguements
            foreach(DataRow dr in dt.Rows)
            {
                int id = int.Parse(dr[2].ToString());
                dr[2] = StaffDBConverter.GetEmployeeNameByID(id);
            }
            return dt;
        }

        internal static void CheckInPatient(int appointmentID) 
        {
            string cmdString = "UPDATE BookedAppointments SET Checked_In = @checkin, Check_In_Time = @time WHERE AppointmentID = @id;";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Parameters.Add("@checkin", DbType.String).Value = "YES";
            cmd.Parameters.Add("@time", DbType.String).Value = DateTime.Now.TimeOfDay.ToString("hhmm", System.Globalization.CultureInfo.GetCultureInfo("en-UK")).Replace(":", "");
            cmd.Parameters.Add("@id", DbType.Int32).Value = appointmentID;
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            conn.Close();
        }

        internal static void UncheckInPatient(int appointmentID)
        {
            string cmdString = "UPDATE BookedAppointments SET Checked_In = @checkin, Check_In_Time = @time WHERE AppointmentID = @id;";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Parameters.Add("@checkin", DbType.String).Value = "NO";
            cmd.Parameters.Add("@time", DbType.String).Value = DateTime.Now.TimeOfDay.ToString("hhmm", System.Globalization.CultureInfo.GetCultureInfo("en-UK")).Replace(":", "");
            cmd.Parameters.Add("@id", DbType.Int32).Value = appointmentID;
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            conn.Close();
        }

        internal static DataTable GetBookedAppointments() 
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("AppointmentID");
            dt.Columns.Add("PatientID");
            dt.Columns.Add("PatientName");
            dt.Columns.Add("AppointmentTime");
            dt.Columns.Add("AppointmentDoctor");
            dt.Columns.Add("AppointmentDate");

            string cmdString = "SELECT BookedAppointments.AppointmentID, BookedAppointments.PatientID, PatientData.Firstname || COALESCE(' ' || PatientData.Middlename, '') || ' ' || PatientData.Lastname AS PatientName, " +
                               "BookedAppointments.Appointment_Time, BookedAppointments.Assigned_DoctorID, BookedAppointments.Date " +
                               "FROM BookedAppointments, PatientData " +
                               "WHERE Checked_In = \'NO\' AND BookedAppointments.PatientID = PatientData.PatientID; ";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            SQLiteDataReader reader = cmd.ExecuteReader();

            while (reader.Read()) 
            {
                DataRow dr = dt.NewRow();
                dr[0] = reader.GetInt32(0);
                dr[1] = reader.GetInt32(1);
                string patientName = reader.GetString(2);
                dr[2] = Regex.Replace(patientName, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
                dr[3] = reader.GetString(3).Insert(reader.GetString(3).Length - 2, ":");
                if (reader.IsDBNull(4))
                    dr[4] = "Priority Appointment.";
                else
                    dr[4] = reader.GetInt32(4);
                dr[5] = reader.GetString(5);
                dt.Rows.Add(dr);
            }
            reader.Close();
            conn.Close();

            cmdString = "SELECT \'Dr. \' || Accounts.Firstname || COALESCE(' ' || Accounts.Middlename, '') || ' ' || Accounts.Lastname AS DoctorName, Id " +
                        "FROM Accounts; ";
            conn = OpenConnection("Staff");
            cmd = new SQLiteCommand(cmdString, conn);
            reader = cmd.ExecuteReader();
            while (reader.Read()) 
            {
                foreach (DataRow dr in dt.Rows) 
                {
                    if (reader.GetInt32(1).ToString() == dr["AppointmentDoctor"].ToString()) 
                    {
                        dr["AppointmentDoctor"] = reader.GetString(0);
                    }
                }
            }

            reader.Close();
            conn.Close();

            DataView dv = dt.DefaultView;
            dv.Sort = "AppointmentTime desc";
            dt = dv.ToTable();

            return dt;
        }

        internal static DataTable GetCancelledAppointments()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("AppointmentID");
            dt.Columns.Add("PatientID");
            dt.Columns.Add("PatientName");
            dt.Columns.Add("AppointmentTime");
            dt.Columns.Add("AppointmentDoctor");
            dt.Columns.Add("AppointmentDate");
            dt.Columns.Add("Reason");

            string cmdString = "SELECT CancelledAppointments.AppointmentID, CancelledAppointments.PatientID, PatientData.Firstname || COALESCE(' ' || PatientData.Middlename, '') || ' ' || PatientData.Lastname AS PatientName, " +
                               "CancelledAppointments.Time, CancelledAppointments.Assigned_DoctorID, CancelledAppointments.Date, CancelledAppointments.Reason " +
                               "FROM CancelledAppointments, PatientData " +
                               "WHERE CancelledAppointments.PatientID = PatientData.PatientID; ";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            SQLiteDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                DataRow dr = dt.NewRow();
                dr[0] = reader.GetInt32(0);
                dr[1] = reader.GetInt32(1);
                string patientName = reader.GetString(2);
                dr[2] = Regex.Replace(patientName, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
                dr[3] = reader.GetString(3).Insert(reader.GetString(3).Length - 2, ":");
                if (reader.IsDBNull(4))
                    dr[4] = "Priority Appointment.";
                else
                    dr[4] = reader.GetInt32(4);
                dr[5] = reader.GetString(5);
                dr[6] = reader.GetString(6);
                dt.Rows.Add(dr);
            }
            reader.Close();
            conn.Close();

            cmdString = "SELECT \'Dr. \' || Accounts.Firstname || COALESCE(' ' || Accounts.Middlename, '') || ' ' || Accounts.Lastname AS DoctorName, Id " +
                        "FROM Accounts; ";
            conn = OpenConnection("Staff");
            cmd = new SQLiteCommand(cmdString, conn);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                foreach (DataRow dr in dt.Rows)
                {
                    if (reader.GetInt32(1).ToString() == dr["AppointmentDoctor"].ToString())
                    {
                        dr["AppointmentDoctor"] = reader.GetString(0);
                    }
                }
            }
            reader.Close();
            conn.Close();

            DataView dv = dt.DefaultView;
            dv.Sort = "AppointmentDate desc";
            dt = dv.ToTable();

            return dt;
        }

        internal static DataTable GetCheckedInAppointments() 
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("AppointmentID");
            dt.Columns.Add("PatientID");
            dt.Columns.Add("PatientName");
            dt.Columns.Add("AppointmentTime");
            dt.Columns.Add("TimeElapsed");
            dt.Columns.Add("AppointmentDoctor");
            dt.Columns.Add("AppointmentDoctorID");
            dt.Columns.Add("isEmergency");
            dt.Columns.Add("isReservation");



            string cmdString = "SELECT BookedAppointments.AppointmentID, BookedAppointments.PatientID, PatientData.Firstname || COALESCE(' ' || PatientData.Middlename, '') || ' ' || PatientData.Lastname AS PatientName, " +
                               "BookedAppointments.Appointment_Time, BookedAppointments.Check_In_Time, BookedAppointments.Assigned_DoctorID, BookedAppointments.Not_Emergency, BookedAppointments.Reservation " +
                               "FROM BookedAppointments, PatientData " +
                               "WHERE Checked_In = \"YES\" AND BookedAppointments.PatientID = PatientData.PatientID; ";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            SQLiteDataReader reader = cmd.ExecuteReader();

            while (reader.Read()) 
            {
                DataRow dr = dt.NewRow();
                dr[0] = reader.GetInt32(0);
                dr[1] = reader.GetInt32(1);
                string patientName = reader.GetString(2);
                dr[2] = Regex.Replace(patientName, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
                dr[3] = reader.GetString(3).Insert(reader.GetString(3).Length - 2, ":");
                dr[4] = reader.GetString(4);
                if (reader.IsDBNull(5))
                {
                    dr[5] = "Priority Appointment.";
                    dr[6] = null;
                }
                else 
                {
                    dr[5] = reader.GetInt32(5);
                    dr[6] = reader.GetInt32(5);
                }
                    
                if (reader.GetString(6) == "YES")
                    dr[7] = false;
                else
                    dr[7] = true;
                if (reader.GetString(7) == "YES")
                    dr[8] = true;
                else
                    dr[8] = false;
                dt.Rows.Add(dr);
            }

            cmdString = "SELECT \'Dr. \' || Accounts.Firstname || COALESCE(' ' || Accounts.Middlename, '') || ' ' || Accounts.Lastname AS DoctorName, Id " +
                       "FROM Accounts; ";
            conn = OpenConnection("Staff");
            cmd = new SQLiteCommand(cmdString, conn);
            reader = cmd.ExecuteReader();
            while (reader.Read()) 
            {
                foreach (DataRow dr in dt.Rows)
                {
                    if (!dr["AppointmentDoctor"].Equals("Priority Appointment.")) 
                    {
                        if (reader.GetInt32(1).ToString() == dr["AppointmentDoctor"].ToString())
                        {
                            dr["AppointmentDoctor"] = reader.GetString(0);
                        }
                    }
                }
            }
            reader.Close();
            conn.Close();

            foreach (DataRow dr in dt.Rows) 
            {
                string checkIn = dr["TimeElapsed"].ToString();
                checkIn = checkIn.Insert(checkIn.Length - 2, ":");
                TimeSpan timeCheckedIn = TimeSpan.Parse(checkIn);
                TimeSpan timeNow = DateTime.Now.TimeOfDay;
                dr["TimeElapsed"] = (timeNow - timeCheckedIn).Minutes.ToString() + " minutes.";
            }

            DataView dv = dt.DefaultView;
            dv.Sort = "isEmergency desc, AppointmentTime desc, TimeElapsed desc";
            dt = dv.ToTable();
            return dt;
        }

        internal static void StartAppointment(DataRow appointmentDetails, int doctorID) 
        {
            string cmdString = "INSERT INTO ActiveAppointments(AppointmentID, \"Date\", Time_Scheduled, PatientID, Patient_Notes, DoctorID, Time_Started) " +
                "VALUES(@appid, @date, " +
                "(SELECT BookedAppointments.Appointment_Time FROM BookedAppointments WHERE AppointmentID = @appid), " +
                "@patid, " +
                "(SELECT BookedAppointments.Patient_Notes FROM BookedAppointments WHERE AppointmentID = @appid), " +
                "@assignedDoctorID, " +
                "@timestart);" +
                "DELETE FROM BookedAppointments WHERE BookedAppointments.AppointmentID = @appid;";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);

            cmd.Parameters.Add("@appid", DbType.Int32).Value = int.Parse(appointmentDetails["AppointmentID"].ToString());
            cmd.Parameters.Add("@date", DbType.String).Value = DateTime.Today.ToShortDateString();
            cmd.Parameters.Add("@patid", DbType.Int32).Value = int.Parse(appointmentDetails["PatientID"].ToString());
            cmd.Parameters.Add("@assignedDoctorID", DbType.Int32).Value = doctorID;
            cmd.Parameters.Add("@timestart", DbType.Int32).Value = int.Parse(DateTime.Now.TimeOfDay.ToString("hhmm"));
            cmd.ExecuteNonQuery();
            conn.Close();
            cmd.Dispose();
            return;
        }


        internal static bool DoctorIsInAppointment(int doctorID) 
        {

            string cmdString = "SELECT COUNT(*) FROM ActiveAppointments WHERE DoctorID = @id";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);

            cmd.Parameters.Add("@id", DbType.Int32).Value = doctorID;

            int recordsFound = int.Parse(cmd.ExecuteScalar().ToString());

            conn.Close();
            cmd.Dispose();

            Console.WriteLine("I think the doctor has this many active appointments: " + recordsFound);
            if (recordsFound > 0)
                return true;
            else
                return false;
        }


        internal static DataRow GetDoctorActiveAppointment(int doctorID) 
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("AppointmentID");
            dt.Columns.Add("Date");
            dt.Columns.Add("Time_Scheduled");
            dt.Columns.Add("PatientID");
            dt.Columns.Add("Patient_Notes");
            dt.Columns.Add("DoctorID");
            dt.Columns.Add("Time_Started");
            DataRow dr = dt.NewRow();

            string cmdString = "SELECT * FROM ActiveAppointments WHERE DoctorID = @id";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);

            cmd.Parameters.Add("@id", DbType.Int32).Value = doctorID;

            SQLiteDataReader reader = cmd.ExecuteReader();

            while (reader.Read()) 
            {
                dr[0] = reader.GetInt32(0);                                                 // Appointment ID   i=0
                dr[1] = reader.GetString(1);                                                // Date             1=1
                dr[2] = reader.GetString(2).Insert(reader.GetString(2).Length - 2, ":");    // Time Scheduled   1=2
                dr[3] = reader.GetInt32(3);                                                 // Patient ID       i=3
                if (reader.IsDBNull(4))                                                     // Patient Notes    i=4
                    dr[4] = "";
                else
                    dr[4] = reader.GetString(4);                                                
                dr[5] = reader.GetInt32(5);                                                 // Doctor ID        i=5
                dr[6] = reader.GetString(6);                                                // Time Started     i=6
            }
            conn.Close();
            cmd.Dispose();

            return dr;
        }
    }
}
