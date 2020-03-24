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
            get { return GetAverage(); }
            set { value = GetAverage(); }
        }

        public static int GetAverage()
        {
            int averageTime;

            //using path in app.config, read firstline of AvgApptDuration.txt (where duration is kept)
            using (StreamReader sr = new StreamReader(path)) 
            { averageTime = int.Parse(sr.ReadLine()); }

            return averageTime;
        }
        public static void SetAverage() 
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
                int shiftDuration = getTimeDifference(starts[element], ends[element]); //error here
                int predictedAppointments = shiftDuration / averageDuration;
                int appointmentsPerHour = 0;
                int currentTime;
                if (isReservation)
                    appointmentsPerHour = (predictedAppointments / (shiftDuration / 60)) / 2; //reservations per hour = average per hour / 2 (For Reservations, avaliable appointments halved)
                else
                    appointmentsPerHour = (predictedAppointments / (shiftDuration / 60)); //reservations per hour = average per hour  (For walk-in, any possible appointment calculated)
                if (isReservation)
                    currentTime = starts[element];
                else 
                {
                    // If the current time is after the doctor starts then the current time is used
                    // however, if the current time is before the start time of any working doctor, then the time is 
                    // set to the ending time of the doctors shift to indicate there is no remaining time to see patients
                    // as the doctor is not actually assumed to be working
                    if (starts[element] < int.Parse(DateTime.Now.ToString("HH:mm").Replace(":", ""))) 
                    {
                        currentTime = int.Parse(DateTime.Now.ToString("HH:mm").Replace(":", ""));
                        // If adding 5 minutes to the current time runs into the next hour, add 45 to number to represent next hour
                        // This is explained in detail below --> but basically, the gist is, if you just take the current time
                        // the system will not recognise it as an avaliable timeslot and will default to postponing when the patient can
                        // be seen. We avoid this by taking the current time THEN adding 5 minutes so the patient is show 5 mins until next appt
                        int minutes = currentTime % 100;
                        if (minutes + 5 > 60)
                            currentTime += 45;
                        else
                            currentTime += 5;
                    }  
                    else
                        currentTime = ends[element];
                }
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

                            int minutes = currentTimeslot % 100;
                            if (minutes + averageDuration < 60)
                                currentTimeslot += averageDuration;
                            else 
                            {
                                // if timeslot will run into next hour, average duration is added, then 60 is taken away (to represent the new hour) + 100 moves to next hour
                                // Please see use cases below where: average duration = 20 & timeslot is first number
                                // 1740 + 20 = 1760 ---> 1760 - 60 = 1700 ---> 1700 + 100 = 1800 ----> 18:00
                                // 1750 + 20 = 1770 ---> 1770 - 60 = 1710 ---> 1710 + 100 = 1810 ----> 18:10
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
            doctorTimeslots.Columns.Add("Doctor_ID");
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

        public static DataRow CalcWalkInTimeslot(DataTable dt) 
        {
            foreach (DataRow dataRow in dt.Rows)
            {
                Console.WriteLine(dataRow[0].ToString() + "         " + dataRow[1].ToString());
                string rowValue = dataRow["Avaliable_Reservations"].ToString();
                Console.WriteLine(rowValue);
                DateTime selectedTime = DateTime.Parse(rowValue);

                //  If time of avaliable appointment is before current time --> remove from options.
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
                EmailSuccess.AlmostReadyEmail(patientID);
            }
            else 
            {
                EmailSuccess.SamedaySuccessEmail(patientID);
                string minutes = timeDifference.Minutes.ToString();
                CallSchedular.ExecuteSchedular(patientID, double.Parse(minutes));
            }
        }
    }
}
