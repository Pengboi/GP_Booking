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
    /// Interaction logic for WaitingListView.xaml
    /// </summary>
    public partial class WaitingListView : UserControl
    {
        public WaitingListView()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Messenger.Default.Send<NotificationMessage>(new NotificationMessage(SearchBox.Text));
        }

    }
}
