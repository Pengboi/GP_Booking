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
        public string Answer 
        {
            get; set;
        }

        public InputDialogBox(string question, int answerLength)
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            OTPQuestion.Content = question;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Answer = Number1.Text + Number2.Text + Number3.Text +
                     Number4.Text + Number5.Text + Number6.Text;
            this.DialogResult = true;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            Number1.SelectAll();
            Number1.Focus();
        }
    }
}
