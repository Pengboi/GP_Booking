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

namespace Schedule_Mgr
{
    /// <summary>
    /// Interaction logic for AccountUsernameInput.xaml
    /// </summary>
    public partial class AccountUsernameInput : Window
    {
        public string Answer
        {
            get; set;
        }

        public AccountUsernameInput(string question)
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            OTPQuestion.Content = question;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Answer = AccountUsername.Text;
            this.DialogResult = true;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            AccountUsername.SelectAll();
            AccountUsername.Focus();
        }
    }
}
