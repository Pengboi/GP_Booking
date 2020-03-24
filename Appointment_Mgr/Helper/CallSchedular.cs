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
