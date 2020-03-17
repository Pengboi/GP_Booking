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

namespace Appointment_Mgr.Model
{
    public static class AppointmentLogic
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
            int averageDuration = getAverage();
            /*
             * Calculate timeslots assuming there are no existing appointments then remove timeslots afterwards 
             * if appointments already exist for each doctor 
             */
            List<List<int>> reservationTimeslots = new List<List<int>>(); // All possible appointments


            for (int element=0; element < starts.Count; element++)
            {
                /*
                 *    - shiftDuration calculates length of each doctors shift in minutes
                 *    - predictedAppointments calculates how many appointments (no remainder) each doctor can see based on dynamic average appointment duration
                 *      & their scheduled work hours.
                 *  Half of predicted possible appointments for each doctor working are reserverd as reservation only, with 50/50 priority on same-day to reservation
                 */
                int shiftDuration = getTimeDifference(starts[element], ends[element]); //error here
                int predictedAppointments = shiftDuration / averageDuration;
                int appointmentsPerHour = 0;
                if (isReservation)
                    appointmentsPerHour = (predictedAppointments / (shiftDuration / 60)) / 2; //reservations per hour = average per hour / 2 (For Reservations, avaliable appointments halved)
                else
                    appointmentsPerHour = (predictedAppointments / (shiftDuration / 60)); //reservations per hour = average per hour  (For walk-in, any possible appointment calculated)
                int currentTime = starts[element];
                int remainingTime = ends[element] - currentTime; //Time left in shift

                Console.WriteLine("Shift Duration: " + shiftDuration + " Predicted Appointments: " + predictedAppointments + " reservations Per Hour: " + appointmentsPerHour);

                List<int> timeslots = new List<int>();
               
                while (currentTime < ends[element])
                {
                    // If more than an hour remains in doctor's shift, add reservation times (otherwise last hour of shift doctor will support walk-in appointments)
                    // I.E: In event where doctor is scheduled 10AM - 5:30PM: 10AM - 5PM Timeslots are calculated, remaining 30minutes doctor will support walk-in
                    if (remainingTime > 100)
                    {
                        int currentTimeslot = currentTime;
                        // Reservation timeslots for next hour of doctor's shift are added to list.
                        for (int i = 0; i < appointmentsPerHour; i++)
                        {
                            timeslots.Add(currentTimeslot);
                            currentTimeslot += averageDuration;
                        }
                    }
                    currentTime += 100; //Add 1 hr        
                }
                reservationTimeslots.Add(timeslots);
            }

            // Removes any timeslots which have already been booked, filtering only avaliable timeslots.
            for (int i=0; i < reservationTimeslots.Count; i++) 
            {
                List<int> existingTimes = bookedTimeslots[i];
                List<int> avaliableTimes = reservationTimeslots[i];

                avaliableTimes.RemoveAll(timeslot => existingTimes.Contains(timeslot)); //lambda expression removes all avaliable times ints that are also in existingtimes for any given doctor
                reservationTimeslots[i] = avaliableTimes;
            }
            
            DataTable doctorTimeslots = new DataTable();
            doctorTimeslots.Columns.Add("Doctor_ID");
            doctorTimeslots.Columns.Add("Avaliable_Reservations");

            for (int i = 0; i < ids.Count; i++) 
            {
                if (isReservation)
                    Console.WriteLine("-----------------------------------RESERVATION TIMES-------------------------------");
                else
                    Console.WriteLine("-----------------------------------WALK IN TIMES-------------------------------");
                for (int j = 0; j < reservationTimeslots[i].Count; j++) 
                {
                    string timeslot = reservationTimeslots[i][j].ToString("D4");
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

        public static DataRow CalcWalkInTimeslot(DataTable dt) 
        {
            foreach (DataRow dataRow in dt.Rows)
            {
                string rowValue = dataRow["Avaliable_Reservations"].ToString();
                TimeSpan selectedTime = TimeSpan.Parse(rowValue);

                //  If time of avaliable appointment is before current time --> remove from options.
                if (DateTime.Now.TimeOfDay < selectedTime)
                {
                    return dataRow;
                }
            }
            return null;
        }
    }
}
