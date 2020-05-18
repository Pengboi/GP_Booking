using System;
using Appointment_Mgr.Dialog;
using Appointment_Mgr.Model;
using System.IO;
using System.Net.Mail;
using System.Windows.Input;
using System.Threading;
using System.Threading.Tasks;

namespace Appointment_Mgr.Helper
{
    public class EmailConfirmation
    {
        private static IDialogBoxService _dialogService;

        public static void ReservationConfirmationEmail(int patientID, DateTime date, string timeslot)
        {
            _dialogService = new DialogBoxService();

            // Modifies email html to input date and time in email.
            DateTime time = DateTime.Parse(timeslot);
            timeslot = time.ToString("hh:mm tt");

            string text = File.ReadAllText("Assets\\appointment_email.html");
            text = text.Replace("InsertDate1", date.ToShortDateString());
            text = text.Replace("HEADING", "We look forward to seeing you.");
            text = text.Replace("MAINBODY", "Your appointment has been confirmed on the date and time below. For any enquiries, please contact the GP.");
            text = text.Replace("InsertDate2", date.ToShortDateString() + ", <br>" + timeslot);
            File.WriteAllText("appointment_email.html", text);

            // Gets patient email
            string patientEmail = PatientDBConverter.GetEmail(patientID);

            try
            {
                using (StreamReader reader = File.OpenText("appointment_email.html"))
                {
                    SmtpClient client = new SmtpClient("smtp.gmail.com");

                    MailMessage message = new MailMessage("noreply@gpsystemuk.com", patientEmail);

                    message.IsBodyHtml = true;
                    message.Body = reader.ReadToEnd();
                    message.Subject = "Your Upcoming GP Appointment.";
                    client.UseDefaultCredentials = false;
                    client.Credentials = new System.Net.NetworkCredential("noreply@gpsystemuk.com", "mitsig-applepie-lambda2000");
                    client.Host = "smtp.gmail.com";
                    client.Port = 587;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.EnableSsl = true;

                    //await 
                    client.SendMailAsync(message);
                }
                File.Delete("appointment_email.html");
            }
            catch (Exception ex)
            {
                // Saves error to error log
                string exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string logPath = exePath + @"\\logs\\log.txt";
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

        public static void WalkInConfirmationEmail(int patientID)
        {
            _dialogService = new DialogBoxService();

            // Modifies email html to input date and time in email.
            string text = File.ReadAllText("Assets\\appointment_email.html");
            text = text.Replace("InsertDate1", DateTime.Today.ToShortDateString());
            text = text.Replace("HEADING", "Your appointment has been booked.");
            text = text.Replace("MAINBODY", "Please keep an eye on your emails for updates on your wait. Should you wish to alter or cancel your appointment please contact the GP.");
            text = text.Replace("InsertDate2", "");

            File.WriteAllText("appointment_email.html", text);

            string patientEmail = PatientDBConverter.GetEmail(patientID);
            try
            {
                using (StreamReader reader = File.OpenText("appointment_email.html"))
                {
                    SmtpClient client = new SmtpClient("smtp.gmail.com");

                    MailMessage message = new MailMessage("noreply@gpsystemuk.com", patientEmail);

                    message.IsBodyHtml = true;
                    message.Body = reader.ReadToEnd();
                    message.Subject = "Your Upcoming GP Appointment.";
                    client.UseDefaultCredentials = false;
                    client.Credentials = new System.Net.NetworkCredential("noreply@gpsystemuk.com", "mitsig-applepie-lambda2000");
                    client.Host = "smtp.gmail.com";
                    client.Port = 587;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.EnableSsl = true;

                    //await 
                    client.SendMailAsync(message);
                    }
                File.Delete("appointment_email.html");
            }
            catch (Exception ex)
            {
                // Saves error to error log
                string exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string logPath = exePath + @"\\logs\\log.txt";
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

        public static void AlmostReadyEmail(int patientID)
        {
            _dialogService = new DialogBoxService();

            // Modifies email html to input date and time in email.
            string text = File.ReadAllText("Assets\\appointment_email.html");
            text = text.Replace("InsertDate1", DateTime.Today.ToShortDateString());
            text = text.Replace("HEADING", "The doctor will see you now.");
            text = text.Replace("MAINBODY", "We're almost ready to see you, please return to the GP within 10 minutes.");
            text = text.Replace("InsertDate2", "");
            File.WriteAllText("appointment_email.html", text);

            string patientEmail = PatientDBConverter.GetEmail(patientID);

            try
            {
                using (StreamReader reader = File.OpenText("appointment_email.html"))
                {
                    SmtpClient client = new SmtpClient("smtp.gmail.com");

                    MailMessage message = new MailMessage("noreply@gpsystemuk.com", patientEmail);

                    message.IsBodyHtml = true;
                    message.Body = reader.ReadToEnd();
                    message.Subject = "We're ready for you.";
                    client.UseDefaultCredentials = false;
                    client.Credentials = new System.Net.NetworkCredential("noreply@gpsystemuk.com", "mitsig-applepie-lambda2000");
                    client.Host = "smtp.gmail.com";
                    client.Port = 587;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.EnableSsl = true;

                    //await 
                    client.SendMailAsync(message);
                }
            }
            catch (Exception ex)
            {

                // Saves error to error log
                string exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string logPath = exePath + @"\\logs\\log.txt";
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
