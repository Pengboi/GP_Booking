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
    /// Interaction logic for InputDialogBox.xaml
    /// </summary>
    public partial class InputDialogBox : Window
    {
        public InputDialogBox(string question, string defaultAnswer = "")
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            OTPQuestion.Content = question;
            OTPAnswer.Text = defaultAnswer;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            OTPAnswer.SelectAll();
            OTPAnswer.Focus();
        }

        public string Answer
        {
            get { return OTPAnswer.Text; }
        }
    }
}
