using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Appointment_Mgr.Model;

namespace Appointment_Mgr.Helper
{
    public class CallSchedular
    {
        // Retrieves patient email and time until they need to be notified before notifying them, process triggers
        // PatientContactSchedular which is a script running in a seperate process/thread in the background unavaliable
        // to the user. Should the current session be effected, by outsourcing patient contact to another process, this will
        // have no reprecussions on patients waiting to be called back.
        public static void ExecuteSchedular(int patientID, double minutes) 
        {
            string email = PatientDBConverter.GetEmail(patientID);

            ProcessStartInfo processInf = new ProcessStartInfo("netcoreapp3.1\\PatientContactSchedular.exe");
            processInf.Arguments = string.Format(email + " " + minutes);
            processInf.WindowStyle = ProcessWindowStyle.Hidden;
            Process run = Process.Start(processInf);
        }
    }
}
