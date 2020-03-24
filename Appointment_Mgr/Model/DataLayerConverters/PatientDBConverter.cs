using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
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
            string cmdString = $"SELECT \"PatientID\" FROM Patient_Data WHERE Firstname = @fname AND" + (!string.IsNullOrWhiteSpace(p.Middlename) ? " Middlename = @mname AND" : "") +
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
            cmd.Dispose();
            sqlda.Dispose();

            return dt;
        }
        public static string GetEmail(int patientID) 
        {
            string cmdString = "SELECT \"E-mail\" FROM Patient_Data WHERE \"PatientID\" = @id";
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
            string cmdString = "UPDATE Patient_Data SET \"E-mail\" = @email WHERE \"PatientID\" = @id";
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

        public static void DeleteRecord(int id)
        {
            string cmdString = @"DELETE FROM Patient_Data WHERE PatientID = @id";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Parameters.Add("@id", DbType.Int32).Value = id;
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public static bool PatientHasAppointment(int patientID, string date = null) 
        {
            if (date == null)
                date = DateTime.Today.ToShortDateString();


            string cmdString = "SELECT COUNT(*) FROM Booked_Appointments WHERE Patient_ID = @id AND Date = @date";
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

        public static void BookAppointment(string timeslot, int doctorID, int patientID, bool isReservation, string notes = null, string date = null) 
        {
            if (date == null)
                date = DateTime.Today.ToShortDateString();

            string cmdString = @"INSERT INTO Booked_Appointments (Date, Patient_ID, Assigned_Doctor_ID, Reservation, Patient_Notes, Appointment_Time)
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

        public static void BookEmergencyAppointment(int patientID) 
        {
            string date = DateTime.Today.ToShortDateString();
            string appointmentTime = DateTime.Now.TimeOfDay.ToString("hhmm", System.Globalization.CultureInfo.GetCultureInfo("en-UK")).Replace(":", "");
            string cmdString = "INSERT INTO Booked_Appointments(Date, Patient_ID, Reservation, Appointment_Time, Checked_In, Check_in_Time, Overridable) " +
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

        public static void DeleteAppointment(int appointmentID) 
        {
            string cmdString = "DELETE FROM Booked_Appointments WHERE AppointmentID = @id;";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);

            cmd.Parameters.Add("@id", DbType.Int32).Value = appointmentID;
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public static DataTable GetAwaitingCheckIn(DateTime dob, string firstname, string doorNo, string postcode) 
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("AppointmentID");
            dt.Columns.Add("AppointmentTime");
            dt.Columns.Add("DoctorName");
            dt.Columns.Add("PatientID");

            // The command begins by retrieving primary key Appointment number, Appointment time, doctor ID and patient Name which is then placed in the above datatable
            // within the specified columns - ID is temporarily placed in the DoctorName column. Only patients who have not been marked as Checked-in are retrieved.
            string cmdString = "SELECT DISTINCT Booked_Appointments.AppointmentID, Booked_Appointments.Appointment_Time, Booked_Appointments.Assigned_Doctor_ID AS DoctorID, " +
                               "Booked_Appointments.Patient_ID FROM Booked_Appointments, Patient_Data " +
                               "WHERE Booked_Appointments.Date = @date AND Booked_Appointments.Checked_In = \"NO\" AND Patient_Data.Firstname = @firstname " +
                               "AND Patient_Data.ST_Number = @drNum AND Patient_Data.Postcode = @postcode AND Patient_Data.DOB = @dob ";
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
            
            // Using the obtained DoctorID, the DataTable is updated with the doctor Name using GetDoctorNameByID - taking the previously obtained ID's as arguements
            foreach(DataRow dr in dt.Rows)
            {
                int id = int.Parse(dr[2].ToString());
                dr[2] = StaffDBConverter.GetDoctorNameByID(id);
            }
            return dt;
        }

        public static void CheckInPatient(int appointmentID) 
        {
            string cmdString = "UPDATE Booked_Appointments SET Checked_In = @checkin, Check_In_Time = @time WHERE AppointmentID = @id;";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            cmd.Parameters.Add("@checkin", DbType.String).Value = "YES";
            cmd.Parameters.Add("@time", DbType.String).Value = DateTime.Now.TimeOfDay.ToString("hhmm", System.Globalization.CultureInfo.GetCultureInfo("en-UK")).Replace(":", "");
            cmd.Parameters.Add("@id", DbType.Int32).Value = appointmentID;
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            conn.Close();
        }


        public static DataTable GetBookedAppointments() 
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("AppointmentID");
            dt.Columns.Add("PatientID");
            dt.Columns.Add("PatientName");
            dt.Columns.Add("AppointmentTime");
            dt.Columns.Add("AppointmentDoctor");
            dt.Columns.Add("AppointmentDate");

            string cmdString = "SELECT Booked_Appointments.AppointmentID, Booked_Appointments.Patient_ID, Patient_Data.Firstname || COALESCE(' ' || Patient_Data.Middlename, '') || ' ' || Patient_Data.Lastname AS PatientName, " +
                               "Booked_Appointments.Appointment_Time, Booked_Appointments.Assigned_Doctor_ID, Booked_Appointments.Date " +
                               "FROM Booked_Appointments, Patient_Data " +
                               "WHERE Checked_In = \'NO\' AND Booked_Appointments.Patient_ID = Patient_Data.PatientID; ";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            SQLiteDataReader reader = cmd.ExecuteReader();

            while (reader.Read()) 
            {
                DataRow dr = dt.NewRow();
                dr[0] = reader.GetInt32(0);
                dr[1] = reader.GetInt32(1);
                dr[2] = reader.GetString(2);
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

        public static DataTable GetCheckedInAppointments() 
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("AppointmentID");
            dt.Columns.Add("PatientID");
            dt.Columns.Add("PatientName");
            dt.Columns.Add("AppointmentTime");
            dt.Columns.Add("TimeElapsed");
            dt.Columns.Add("AppointmentDoctor");



            string cmdString = "SELECT Booked_Appointments.AppointmentID, Booked_Appointments.Patient_ID, Patient_Data.Firstname || COALESCE(' ' || Patient_Data.Middlename, '') || ' ' || Patient_Data.Lastname AS PatientName, " +
                               "Booked_Appointments.Appointment_Time, Booked_Appointments.Check_In_Time, Booked_Appointments.Assigned_Doctor_ID " +
                               "FROM Booked_Appointments, Patient_Data " +
                               "WHERE Checked_In = \"YES\" AND Booked_Appointments.Patient_ID = Patient_Data.PatientID; ";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
            SQLiteDataReader reader = cmd.ExecuteReader();

            while (reader.Read()) 
            {
                DataRow dr = dt.NewRow();
                dr[0] = reader.GetInt32(0);
                dr[1] = reader.GetInt32(1);
                dr[2] = reader.GetString(2);
                dr[3] = reader.GetString(3).Insert(reader.GetString(3).Length - 2, ":");
                dr[4] = reader.GetString(4);
                if (reader.IsDBNull(5))
                    dr[5] = "Priority Appointment.";
                else
                    dr[5] = reader.GetInt32(5);
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
            dv.Sort = "AppointmentTime desc, TimeElapsed desc";
            dt = dv.ToTable();
            foreach (DataRow dataRow in dt.Rows)
            {
                foreach (var item in dataRow.ItemArray)
                {
                    Console.WriteLine(item);
                }
            }
            return dt;
        }
    }
}
