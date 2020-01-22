using Appointment_Mgr.Helper;
using Appointment_Mgr.ViewModel;
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
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {   
        // NOTE --> All commented out code has been replaced with custom dialog box. Code just serves as legacy for indication on if alt solution needed.
        public LoginView()
        {
            InitializeComponent();
            // NON MVVM --> Used here due to weaknesses in WPF framework with XAML ---> Unable to currently call messagebox from ViewModel 
            // so MVVM is seem as guide during this project as oppose to STRICT rules to be adhered to. MVVM to reduce coupling, not eliminate.
                // Receives error message and sends to GetErrorMessage to find appropriate message
            
            
        }

        /// <summary>
        /// Password boxes cannot send data by themselves so some code-behind was needed to transalte password box from encrypted SecureString to plaintext
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Not using Secure String --> as attacks are only possible if user has access to RAM, too long to fix. Unfeasable.
            if (this.DataContext != null)
            { ((dynamic)this.DataContext).Password = ((PasswordBox)sender).Password; }
        }
    }
}
