using Appointment_Mgr.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Appointment_Mgr.ViewModel
{
    public class DoctorHomeToolbarViewModel : ViewModelBase
    {
        private string _userLoggedIn;
        private string _clockTime = DateTime.Now.ToString("HH:mm"); private string _dateValue = DateTime.Now.ToString("dd/MM/yy");
        private DispatcherTimer timer;

        public string UserLoggedIn
        {
            get { return _userLoggedIn; }
            set 
            {
                value = _userLoggedIn;
                RaisePropertyChanged(nameof(UserLoggedIn));
            }
        }

        #region Clock elements
        public string LiveClock
        {
            get { return this._clockTime; }
            set
            {
                if (this._clockTime == value)
                    return;
                this._clockTime = value;
                RaisePropertyChanged(nameof(LiveClock));
            }
        }
        public string LiveDate
        {
            get { return this._dateValue; }
            set
            {
                if (this._dateValue == value)
                    return;
                this._dateValue = value;
                RaisePropertyChanged(nameof(LiveDate));
            }
        }
        #endregion

        public RelayCommand ExecuteLogout { get; }

        public DoctorHomeToolbarViewModel() 
        {
            if (IsInDesignMode) 
            {
                LiveClock = "15:45";
                LiveDate = "05/09/16";
                UserLoggedIn = "Dr. Page Madison";
            }
            timer = new DispatcherTimer(DispatcherPriority.Normal);
            timer.Interval = TimeSpan.FromSeconds(0.3);
            timer.Tick += (sender, args) =>
            {
                LiveClock = DateTime.Now.ToString("HH:mm");
                LiveDate = DateTime.Now.ToString("dd/MM/yy");
            };
            timer.Start();

            Messenger.Default.Register<int>(this, SetUserLoggedIn);
            UserLoggedIn = _userLoggedIn;
            Console.WriteLine("SO I GOT IT THO: " + _userLoggedIn);
            ExecuteLogout = new RelayCommand(ExecuteLogoutCommand);
        }

        private void SetUserLoggedIn(int id)
        {
            _userLoggedIn = StaffDBConverter.GetDoctorNameByID(id);
        }

        private void ExecuteLogoutCommand()
        {
            MessengerInstance.Send<string>("HomeView");
            MessengerInstance.Unregister(this); // moves messenger to garbage collection
            ViewModelLocator.Cleanup();
        }
    }
}
