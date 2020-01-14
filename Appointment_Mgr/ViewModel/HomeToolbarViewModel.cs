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
    public class HomeToolbarViewModel : ViewModelBase
    {
        private string _clockTime = DateTime.Now.ToString("HH:mm"); private string _dateValue = DateTime.Now.ToString("dd/MM/yy");
        private string _userLogin = "Login";
        private DispatcherTimer timer;

        public RelayCommand ShowLoginCommand { private set; get; }

        public string LiveClock
        {
            get { return this._clockTime; }
            set
            {
                if (this._clockTime == value)
                    return;
                this._clockTime = value;
                RaisePropertyChanged("LiveClock");
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
                RaisePropertyChanged("LiveDate");
            }
        }
        public string UserLogin
        {
            get { return this._userLogin; }
            set
            {
                this._userLogin = value;
                RaisePropertyChanged("UserLogin");
            }
        }

        public void LoginCommandMethod()
        {
            MessengerInstance.Send<NotificationMessage>(new NotificationMessage("LoginView"));
        }

        public HomeToolbarViewModel() 
        {
            if (IsInDesignMode)
            {
                LiveClock = "15:45";
                LiveDate = "05/09/16";
                UserLogin = _userLogin;

            }
            else
            {
                timer = new DispatcherTimer(DispatcherPriority.Render);
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += (sender, args) =>
                {
                    LiveClock = DateTime.Now.ToString("HH:mm");
                    LiveDate = DateTime.Now.ToString("dd/MM/yy");
                };
                timer.Start();
                UserLogin = _userLogin;

            }
            ShowLoginCommand = new RelayCommand(LoginCommandMethod);
        }
    }
}
