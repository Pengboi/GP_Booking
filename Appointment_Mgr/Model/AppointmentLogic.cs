using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Data.SQLite;
using System.Globalization;
using System.Data;
using Appointment_Mgr.Helper;

namespace Appointment_Mgr.Model
{
    public static class AppointmentLogic
    {
        private static string LoadConnectionString(string id) {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString.Replace("{AppDir}", AppDomain.CurrentDomain.BaseDirectory); 
        }
        private static SQLiteConnection OpenConnection(string id = "Patients")
        {

            SQLiteConnection connection = new SQLiteConnection(LoadConnectionString(id), true);
            connection.Open();
            return connection;
        }

        public static int averageDuration 
        {
            get { return GetAverage(); }
            set { value = GetAverage(); }
        }

        public static int GetAverage()
        {
            int calculatedAverage;

            SQLiteConnection conn = OpenConnection();
            string cmdString = "SELECT AVG(Appointment_Duration) FROM CompletedAppointments";
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);


            if (string.IsNullOrWhiteSpace(cmd.ExecuteScalar().ToString()) || cmd.ExecuteScalar().ToString().Equals("0")) 
            {
                cmd.Cancel();
                conn.Close();
                return 1;
            }

            decimal result = Math.Round(decimal.Parse(cmd.ExecuteScalar().ToString()));
            
            calculatedAverage = (int)result;

            cmd.Cancel();
            conn.Close();

            return calculatedAverage;
        }


        public static int getTimeDifference(int a, int b) 
        {
            // pads strings with leading 0 if time is less than 4 digits i.e. time is "900" for 9AM --> "0900"
            DateTime startTime = DateTime.ParseExact(a.ToString().PadLeft(4, '0'), "HHmm", CultureInfo.InvariantCulture);
            DateTime endTime = DateTime.ParseExact(b.ToString().PadLeft(4, '0'), "HHmm", CultureInfo.InvariantCulture);

            return (int)endTime.Subtract(startTime).TotalMinutes;
        }

        public static DataTable CalcTimeslots(List<int> ids, List<int> starts, List<int> ends, List<List<int>> bookedTimeslots, bool isReservation) 
        {
            // Gets average appointment time
            int averageDuration = GetAverage();
            /*
             * Calculate timeslots assuming there are no existing appointments then remove timeslots afterwards 
             * if appointments already exist for each doctor 
             */
            List<List<int>> appointmentTimeslots = new List<List<int>>(); // All possible appointments


            for (int element=0; element < starts.Count; element++)
            {
                /*
                 *    - shiftDuration calculates length of each doctors shift in minutes
                 *    - predictedAppointments calculates how many appointments (no remainder) each doctor can see based on dynamic average appointment duration
                 *      & their scheduled work hours.
                 *  Half of predicted possible appointments for each doctor working are reserverd as reservation only, with 50/50 priority on same-day to reservation
                 */
                int shiftDuration = getTimeDifference(starts[element], ends[element]); 
                int predictedAppointments = (int)Math.Round((double)shiftDuration / (double)averageDuration);
                int appointmentsPerHour = 0;
                int currentTime;
                if (isReservation)
                    appointmentsPerHour = (predictedAppointments / (shiftDuration / 60)) / 2;
                else
                    appointmentsPerHour = (predictedAppointments / (shiftDuration / 60)); 
                if (isReservation)
                    currentTime = starts[element];
                else 
                {
                    // If the current time is before the doctors start shift, their start shift is used as earliest avaliable time to be seen
                    if (starts[element] >= int.Parse(DateTime.Now.ToString("HH:mm").Replace(":", ""))) 
                    {
                        currentTime = starts[element];
                    }
                    // If the current time is after the doctor starts then the current time is used
                    // however, if the current time is before the start time of any working doctor, then the time is 
                    // set to the ending time of the doctors shift to indicate there is no remaining time to see patients
                    // as the doctor is not actually assumed to be working.
                    else
                    {
                        // if current time is after doctors end time
                        if (ends[element] < int.Parse(DateTime.Now.ToString("HH:mm").Replace(":", "")))
                            currentTime = ends[element];
                        else 
                        {
                            currentTime = int.Parse(DateTime.Now.ToString("HH:mm").Replace(":", ""));
                            // A 5 minute buffer is used if a patient can be expected to be seen immediately.
                            // If adding 5 minutes to the current time runs into the next hour, add 45 to number to represent next hour + 5 minutes
                            int minutes = currentTime % 100;
                            if (minutes + 5 > 60)
                                currentTime += 45;
                            else
                                currentTime += 5;
                        }
                    }  
                }
                int remainingTime = ends[element] - currentTime; //Time left in shift

                Console.WriteLine("Shift Duration: " + shiftDuration + " Predicted Appointments: " + predictedAppointments + " reservations Per Hour: " + appointmentsPerHour);

                List<int> timeslots = new List<int>();
               
                while (currentTime < ends[element])
                {
                    /*
                     *  If calculating reservation avaliability, last hour of doctors shift should not be used for timeslots to allow doctor to support walk-in queue
                     *  If calculating walk-in avaliability, timeslots should be calculated unless remaining shift duration is shorter than the average appointment duration + 15 minutes
                     *  The 15 minutes in the second scenario serves as a precautionary buffer
                     */
                    int appointmentCutOff;
                    if (isReservation) 
                        appointmentCutOff = 100;
                    else 
                        appointmentCutOff = averageDuration + 15;

                    if (remainingTime > appointmentCutOff)
                    {
                        int currentTimeslot = currentTime;
                        // Reservation timeslots for next hour of doctor's shift are added to list.
                        for (int i = 0; i < appointmentsPerHour; i++)
                        {
                            timeslots.Add(currentTimeslot);

                            int minutes = currentTimeslot % 100;
                            if (minutes + averageDuration < 60)
                                currentTimeslot += averageDuration;
                            else 
                            {
                                // if timeslot will run into next hour, average duration is added, then 60 is taken away (to represent the new hour)
                                // Please see use cases below where: average duration = 20 & timeslot is first number
                                // 1750 + 20 = 1770 ---> 1770 - 60 = 1710 ---> 1710 + 100 = 1810 ----> 18:10 same as 17:50 + 20 minutes
                                currentTimeslot += averageDuration + 40;
                            }
                        }
                    }
                    currentTime += 100; //Add 1 hr        
                }
                appointmentTimeslots.Add(timeslots);
            }

            // Removes any timeslots which have already been booked, filtering only avaliable timeslots.
            for (int i=0; i < appointmentTimeslots.Count; i++) 
            {
                List<int> existingTimes = bookedTimeslots[i];

                // If calculations are occuring for walk-in times, then the estimated duration of each appointment
                // must also be considered as an "unavaliable time"
                // I.e. if an appointment exists at 11:00AM and the average duration is 20 mins
                // Times from 11:00 - 11:20 must be added to existing times.
                if (isReservation == false)
                {
                    int existingCount = existingTimes.Count;
                    for (int j = 0; j < existingCount; j++)
                    {
                        existingTimes.Add(existingTimes[j] + 1);

                        for (int k = 1; k <= averageDuration; k++)
                            existingTimes.Add(existingTimes[j] + k);
                    }
                }
                List<int> avaliableTimes = appointmentTimeslots[i];
                avaliableTimes.RemoveAll(timeslot => existingTimes.Contains(timeslot)); //lambda expression removes all avaliable times ints that are also in existingtimes for any given doctor
                appointmentTimeslots[i] = avaliableTimes;
            }

            DataTable doctorTimeslots = new DataTable();
            doctorTimeslots.Columns.Add("DoctorID");
            doctorTimeslots.Columns.Add("Avaliable_Reservations");

            for (int i = 0; i < ids.Count; i++) 
            {
                if (isReservation)
                    Console.WriteLine("-----------------------------------RESERVATION TIMES-------------------------------");
                else
                    Console.WriteLine("-----------------------------------WALK IN TIMES-------------------------------");
                for (int j = 0; j < appointmentTimeslots[i].Count; j++) 
                {
                    string timeslot = appointmentTimeslots[i][j].ToString("D4");
                    timeslot = timeslot.Insert(timeslot.Length - 2, ":");
                    doctorTimeslots.Rows.Add(ids[i], timeslot);
                    Console.WriteLine("ROW ------ ID: " + ids[i].ToString() + "   TIMESLOTS: " + timeslot.ToString());
                }
            }

            // Code sorts datatable based on timeslots in ascending order.
            DataView sortTimeslots = doctorTimeslots.DefaultView;
            sortTimeslots.Sort = "Avaliable_Reservations asc";
            doctorTimeslots = sortTimeslots.ToTable();

            return doctorTimeslots;
        }


        public static DataRow CalcWalkInTimeslot() 
        {
            DataTable workingDoctors = StaffDBConverter.GetWorkingDoctors(DateTime.Today.Date, "None", "None");

            // Doctor shift start & end times stored to be used to calculate appointment possibilities
            List<int> staffID = new List<int>();
            List<int> shiftStarts = new List<int>();
            List<int> shiftEnds = new List<int>();
            foreach (DataRow row in workingDoctors.Rows)
            {
                staffID.Add(int.Parse(row["Id"].ToString()));
                shiftStarts.Add(int.Parse(row["Shift_Start"].ToString()));
                shiftEnds.Add(int.Parse(row["Shift_End"].ToString()));
            }

            DataTable dt = CalcTimeslots(staffID, shiftStarts, shiftEnds, PatientDBConverter.GetBookedTimeslots(DateTime.Today.Date, staffID), false);

            foreach (DataRow dataRow in dt.Rows)
            {
                string rowValue = dataRow["Avaliable_Reservations"].ToString();
                DateTime selectedTime = DateTime.Parse(rowValue);

                //  Returns earliest timeslot after the current time
                if (DateTime.Now.TimeOfDay < selectedTime.TimeOfDay)
                {
                    return dataRow;
                }
            }
            return null;
        }


        public static void ScheduleWalkInNotification(TimeSpan timeslot, int patientID) 
        {
            TimeSpan timeNow = DateTime.Now.TimeOfDay;

            TimeSpan timeDifference = timeslot - timeNow;

            if (timeDifference.Hours == 0 && timeDifference.Minutes < 10)
            {
                EmailConfirmation.AlmostReadyEmail(patientID);
            }
            else 
            {
                EmailConfirmation.WalkInConfirmationEmail(patientID);
                string minutes = timeDifference.Minutes.ToString();
                CallSchedular.ExecuteSchedular(patientID, double.Parse(minutes));
            }
        }

        public static void EndAppointment(DataRow dr, int appointmentDuration)
        {
            string cmdString = "INSERT INTO CompletedAppointments(AppointmentID, \"Date\", PatientID, Patient_Notes, DoctorID, Appointment_Duration) " +
                "VALUES(@appid, @date, @patID, (SELECT ActiveAppointments.Patient_Notes FROM ActiveAppointments WHERE ActiveAppointments.AppointmentID = @appid), @docID, @duration);" +
                "DELETE FROM ActiveAppointments WHERE ActiveAppointments.AppointmentID = @appid;";
            SQLiteConnection conn = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(cmdString, conn);

            cmd.Parameters.Add("@appid", DbType.Int32).Value = int.Parse(dr[0].ToString());
            cmd.Parameters.Add("@date", DbType.String).Value = dr[1].ToString();
            cmd.Parameters.Add("@patID", DbType.Int32).Value = int.Parse(dr[3].ToString());
            cmd.Parameters.Add("@docID", DbType.Int32).Value = int.Parse(dr[5].ToString());
            cmd.Parameters.Add("@duration", DbType.Int32).Value = appointmentDuration;

            cmd.ExecuteNonQuery();
            conn.Close();
            cmd.Dispose();
            return;
        }
    }
}
