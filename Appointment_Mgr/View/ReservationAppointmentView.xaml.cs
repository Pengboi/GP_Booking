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
    /// Interaction logic for ReservationAppointmentView.xaml
    /// </summary>
    public partial class ReservationAppointmentView : UserControl
    {
        public ReservationAppointmentView()
        {
            InitializeComponent();
        }

        //Violates MVVM Single Respondsibility

        private void FormatCalendar(object sender, RoutedEventArgs e)
        {
            Calendar.DisplayDateStart = DateTime.Today.AddDays(1);
            Calendar.DisplayDateEnd = DateTime.Today.AddDays(14);

            DateTime selectedDay = DateTime.Today.AddDays(1);
            DateTime maxDay = DateTime.Today.AddDays(15);

            while (selectedDay != maxDay)
            {
                if ((selectedDay.DayOfWeek == DayOfWeek.Saturday) || (selectedDay.DayOfWeek == DayOfWeek.Sunday))
                {
                    Calendar.BlackoutDates.Add(new CalendarDateRange(selectedDay));
                }
                selectedDay = selectedDay.AddDays(1);
            }
        }
    }
}
