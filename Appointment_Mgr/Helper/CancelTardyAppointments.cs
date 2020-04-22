using System;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Data;
using System.Threading.Tasks;

namespace Appointment_Mgr.Helper
{
    public static class CancelTardyAppointments
    {
        private static string LoadConnectionString(string id) { return ConfigurationManager.ConnectionStrings[id].ConnectionString; }
        public static SQLiteConnection OpenConnection(string id = "Patients")
        {
            SQLiteConnection connection = new SQLiteConnection(LoadConnectionString(id));
            connection.Open();
            return connection;
        }

        public static async Task Start()
        {
            try
            {
                await Task.Run(() =>
                {
                    // While application is active, every minute a connection occurs with Booked_Appointments Schema and if 15 minutes passes
                    // from when the patient is expected to be seen the appointment is deleted from the Database in order to free avaliability
                    for (; ; )
                    {
                        string cmdString = "SELECT COUNT(*) FROM Booked_Appointments WHERE Date= @date AND @time - Appointment_Time > 15 AND Checked_In = \"NO\";";
                        SQLiteConnection conn = OpenConnection();
                        SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);
                        cmd.Parameters.Add("@date", DbType.String).Value = DateTime.Today.ToShortDateString();
                        cmd.Parameters.Add("@time", DbType.Int32).Value = DateTime.Now.TimeOfDay.ToString("hhmm");
                        
                        int recordsFound = int.Parse(cmd.ExecuteScalar().ToString());
                        cmd.Dispose();
                        if (recordsFound > 0) 
                        {
                            cmdString = "INSERT INTO Cancelled_Appointments " +
                                        "VALUES(" +
                                              "(SELECT AppointmentID FROM Booked_Appointments WHERE Date = @date AND @time - Appointment_Time > 15 AND Checked_In = \"NO\"), " +
                                              "(SELECT Date FROM Booked_Appointments WHERE Date = @date AND @time - Appointment_Time > 15 AND Checked_In = \"NO\"), " +
                                              "(SELECT Appointment_Time FROM Booked_Appointments WHERE Date = @date AND @time - Appointment_Time > 15 AND Checked_In = \"NO\"), " +
                                              "(SELECT Patient_ID FROM Booked_Appointments WHERE Date = @date AND @time - Appointment_Time > 15 AND Checked_In = \"NO\"), " +
                                              "(SELECT Patient_Notes FROM Booked_Appointments WHERE Date = @date AND @time - Appointment_Time > 15 AND Checked_In = \"NO\"), " +
                                              "(SELECT Assigned_Doctor_ID FROM Booked_Appointments WHERE Date = @date AND @time - Appointment_Time > 15 AND Checked_In = \"NO\")," +
                                              "\"Tardy\"" +
                                              ");" +
                                        "DELETE FROM Booked_Appointments " +
                                        "WHERE Date = @date AND @time - Appointment_Time > 15 AND Checked_In = \"NO\"; ";
                            SQLiteCommand cmd2 = new SQLiteCommand(cmdString, conn);
                            cmd2.Parameters.Add("@date", DbType.String).Value = DateTime.Today.ToShortDateString();
                            cmd2.Parameters.Add("@time", DbType.Int32).Value = DateTime.Now.TimeOfDay.ToString("hhmm");
                            cmd2.ExecuteNonQuery();
                            cmd2.Dispose();
                            
                        }
                        conn.Close();
                        System.Threading.Thread.Sleep(60000); // After execution, thread is timed out for 1 minute (60000 in milliseconds)
                    }
                }).ConfigureAwait(false);
            }
            // Should an exception occur, unless non-fatal, application workflow should not be interupted and instead exception is dumped
            // to logs.
            catch (Exception ex)
            {
                string exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string parentPath = Directory.GetParent(exePath).ToString();

                string logPath = parentPath + "\\logs\\log.txt";
                using (StreamWriter writer = new StreamWriter(logPath, true))
                {
                    writer.WriteLine("-----------------------------------------------------------------------------");
                    writer.WriteLine("Date : " + DateTime.Now.ToString());
                    writer.WriteLine();

                    while (ex != null)
                    {
                        writer.WriteLine(ex.GetType().FullName);
                        writer.WriteLine("Message : " + ex.Message);
                        writer.WriteLine("StackTrace : " + ex.StackTrace);

                        ex = ex.InnerException;
                    }
                }
            }
        }
    }
}
