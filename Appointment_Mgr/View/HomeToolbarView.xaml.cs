using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Appointment_Mgr.View
{
    /// <summary>
    /// Interaction logic for HomeTooltipView.xaml
    /// </summary>
    public partial class HomeToolbarView : UserControl
    {
        public HomeToolbarView()
        {
            InitializeComponent();
            Messenger.Default.Register<NotificationMessage>(this, NotificationMessageReceived);
        }
        private void NotificationMessageReceived(NotificationMessage msg)
        {
            if (msg.Notification == "LoginView")
            {
                var loginView = new LoginView();
                loginView.ShowDialog();
            }
        }
    }
}
