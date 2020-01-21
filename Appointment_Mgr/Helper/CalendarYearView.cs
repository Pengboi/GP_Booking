using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Appointment_Mgr.Helper
{
    // Code from https://stackoverflow.com/questions/24624474/wpf-datepicker-start-from-year-in-calendar

    public class CalendarYearView : DependencyObject
    {
        public static CalendarMode GetDisplayMode(DependencyObject calendar)
        {
            return (CalendarMode)calendar.GetValue(DisplayModeProperty);
        }

        public static void SetDisplayMode(DependencyObject calendar, CalendarMode value)
        {
            calendar.SetValue(DisplayModeProperty, value);
        }

        // Using a DependencyProperty as the backing store for DisplayMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayModeProperty =                                                            //was CalendarMode.Month --> Changed to decade so it would show decade first.
            DependencyProperty.RegisterAttached("DisplayMode", typeof(CalendarMode), typeof(CalendarYearView), new PropertyMetadata(CalendarMode.Decade, OnModeChanged));

        private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Calendar cal = d as Calendar;
            cal.IsKeyboardFocusWithinChanged += (ss, ee) => cal.SetValue(Calendar.DisplayModeProperty, e.NewValue);
        }
    }
}
