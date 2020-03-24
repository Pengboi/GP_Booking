using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Appointment_Mgr.Dialog
{
    /// <summary>
    /// Interaction logic for PatientIDBoxView.xaml
    /// </summary>
    public partial class PatientIDBoxView : UserControl
    {
        public PatientIDBoxView()
        {
            InitializeComponent();
        }

        // Previews text input by user, if text inputted is not numerical, it is not accepted.
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        // Previews processes executed within the textbox. If the user attempts to cut, copy or paste
        // the process is handled and the value is set to null (as defined in xaml) - meaning cut, copy return null
        // paste accepts null.
        private void TextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }
    }
}
