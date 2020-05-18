using System;
using System.IO;
using System.Net.Mail;
using System.Threading;
using System.Timers;

namespace PatientContactSchedular
{
    // code idea taken from https://docs.microsoft.com/en-us/dotnet/api/system.timers.timer?redirectedfrom=MSDN&view=netframework-4.8
    // documentation on system.timers

    class Program
    {
        private static System.Timers.Timer timer;
        private static string email;
        private static double minutes;
        private static bool patientContacted;

        static void Main(string[] args)
        {
            patientContacted = false;
            email = args[0];
            minutes = double.Parse(args[1]);
            // If patient estimated to wait 20 or more minutes, they are notified to return 15 minutes before when they are expected to be seen.
            // E.G. Patient appointment expected in 35 mins, patient receives email in 20 mins to return.
            if (minutes >= 20) 
            {
                minutes -= 15;
            }
            var timeToNotify = TimeSpan.FromMinutes(minutes);
            var timer = new System.Threading.Timer((e) =>
            {
                ContactPatient();
            }, null, timeToNotify, timeToNotify);
            Console.ReadLine();
        }


        private static void ContactPatient()
        {
            string exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string parentPath = Directory.GetParent(exePath).ToString();

            string text = File.ReadAllText(parentPath + "\\Assets\\appointment_email.html");
            text = text.Replace("InsertDate1", DateTime.Today.ToShortDateString());
            text = text.Replace("HEADING", "The doctor will see you now.");
            text = text.Replace("MAINBODY", "We're almost ready to see you, please return to the GP within 10 minutes.");
            text = text.Replace("InsertDate2", "");
            File.WriteAllText("appointment_email.html", text);

            try
            {
                using (StreamReader reader = File.OpenText("appointment_email.html"))
                {
                    SmtpClient client = new SmtpClient("smtp.gmail.com");

                    MailMessage message = new MailMessage("noreply@gpsystemuk.com", email);

                    message.IsBodyHtml = true;
                    message.Body = reader.ReadToEnd();
                    message.Subject = "We're ready for you.";
                    client.UseDefaultCredentials = false;
                    client.Credentials = new System.Net.NetworkCredential("noreply@gpsystemuk.com", "mitsig-applepie-lambda2000");
                    client.Host = "smtp.gmail.com";
                    client.Port = 587;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.EnableSsl = true;

                    client.Send(message);
                }
                File.Delete("appointment_email.html");
            }
            catch (Exception ex)
            {
                // Saves error to error log
                
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
            Environment.Exit(0);
        }
    }
}
