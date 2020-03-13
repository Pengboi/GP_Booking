using System;
using Appointment_Mgr.Dialog;
using Appointment_Mgr.Model;
using System.IO;
using System.Net.Mail;
using System.Windows.Input;

namespace Appointment_Mgr.Helper
{
    public class EmailConfirmation
    {
        private static IDialogBoxService _dialogService;
        public ICommand ErrorCommand { get; private set; }

        public static void ReservationEmail(int patientID, DateTime date, string timeslot)
        {
            _dialogService = new DialogBoxService();

            // Modifies email html to input date and time in email.
            DateTime time = DateTime.Parse(timeslot);
            timeslot = time.ToString("hh:mm tt");

            string text = File.ReadAllText("Assets\\reservation_email.html");
            text = text.Replace("InsertDate1", date.ToShortDateString());
            text = text.Replace("InsertDate2", date.ToString("dd MMMM yyyy") + ", " + timeslot);
            File.WriteAllText("reservation_email.html", text);

            // Gets patient email
            Console.WriteLine(patientID);
            string patientEmail = PatientDBConverter.GetEmail(patientID);

            try
            {
                using (StreamReader reader = File.OpenText("reservation_email.html"))
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

                    client.Send(message);
                    Console.WriteLine("Mail delivered successfully!!!");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                var dialog = new Dialog.Error.ErrorBoxViewModel("Error!", "E-mail confirmation could not be sent, try again later. If issue persists, please speak to IT. \n " +
                    "Appointment has been booked and is not effected by this error.");
                var result = _dialogService.OpenDialog(dialog);

                // Saves error to error log
                using (StreamWriter writer = new StreamWriter("logs\\", true))
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
