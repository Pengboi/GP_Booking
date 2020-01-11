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
using System.Windows.Shapes;

namespace Appointment_Mgr.Dialog
{
    /// <summary>
    /// Interaction logic for ErrorMessageTemplate.xaml
    /// </summary>
    public partial class DialogBoxView : Window
    {
        public DialogBoxView()
        {
            InitializeComponent();
            Messenger.Default.Register<NotificationMessage>(this, CloseDialog);
        }

        private void CloseDialog(NotificationMessage msg)
        {
            if (msg.Notification == "CloseDialog")
            {
                this.Close();
            }
        }
    }
}
