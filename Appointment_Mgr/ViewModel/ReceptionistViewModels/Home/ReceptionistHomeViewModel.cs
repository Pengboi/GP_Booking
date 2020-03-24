using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Mgr.ViewModel
{
    public class ReceptionistHomeViewModel : ViewModelBase
    {
        public string ScreenMessage { get; set; }
        public RelayCommand BookAppointmentCommand { private set; get; }
        public RelayCommand EmergencyBookAppointmentCommand { private set; get; }
        public RelayCommand CheckInCommand { private set; get; }

        public ReceptionistHomeViewModel()
        {
            if (IsInDesignMode)
            {
                ScreenMessage = "Placeholder";
            }
            else 
            {
                ScreenMessage = "Placeholder";
            }
            BookAppointmentCommand = new RelayCommand(SetBookingView);
            EmergencyBookAppointmentCommand = new RelayCommand(SetEmergencyBookingVIew);
            CheckInCommand = new RelayCommand(SetCheckInView);
        }

        public void SetBookingView()
        {
            MessengerInstance.Send<string>("BookingView");
            Cleanup();
        }
        private void SetEmergencyBookingVIew()
        {
            MessengerInstance.Send<string>("EmergencyBookingView");
            Cleanup();
        }
        public void SetCheckInView()
        {
            MessengerInstance.Send<string>("CheckInView");
            Cleanup();
        }

        public override void Cleanup()
        {
            MessengerInstance.Unregister(this);
            base.Cleanup();
            ViewModelLocator.Cleanup();
        }
    }
}
