using System;
using System.Windows;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Data.SqlClient;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Data.SQLite;
using System.Data;
using System.Configuration;
using OtpNet;

namespace Schedule_Mgr
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ManagerWindow : Window
    {
        public ManagerWindow()
        {
            InitializeComponent();
        }

        private static string LoadConnectionString(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }


        private void calendar_CalendarFormat(object sender, RoutedEventArgs e) 
        {   
            calendar.DisplayDateStart = DateTime.Today;
            calendar.DisplayDateEnd = DateTime.Today.AddDays(14);

            DateTime selectedDay = DateTime.Today;
            DateTime maxDay = DateTime.Today.AddDays(14);

            while (selectedDay != maxDay) 
            {
                if ((selectedDay.DayOfWeek == DayOfWeek.Saturday) || (selectedDay.DayOfWeek == DayOfWeek.Sunday)) 
                {
                    calendar.BlackoutDates.Add(new CalendarDateRange(selectedDay));
                }
                selectedDay = selectedDay.AddDays(1);
            }
        }


        private void employeeList_GetDoctors(object sender, RoutedEventArgs e) 
        {
            String sqlPath = LoadConnectionString();
            SQLiteConnection connection = new SQLiteConnection(sqlPath);
            string sqlQuery = $"SELECT Firstname, Middlename, Lastname FROM Accounts WHERE Account_Type = 2;";

            connection.Open();
            var cmd = new SQLiteCommand(sqlQuery, connection);
            SQLiteDataReader reader = cmd.ExecuteReader();

            while (reader.Read()) 
            {
                string middlename = "";
                if (!(reader["Middlename"] == null))
                    middlename = reader["Middlename"].ToString();
                
                string name = "Dr. " + reader["Firstname"].ToString() + " " + middlename + " " + reader["Lastname"].ToString();
                employeeList.Items.Add(name);
            }
        }


        private void comboBox_SetTimes(object sender, RoutedEventArgs e) 
        {
            for (int i = 0; i < 24; i++) 
            {
                hoursComboBox.Items.Add(i.ToString("00"));
                hoursComboBox2.Items.Add(i.ToString("00"));
            }
            for (int i = 0; i < 60; i = i + 5) 
            {
                minsComboBox.Items.Add(i.ToString("00"));
                minsComboBox2.Items.Add(i.ToString("00"));
            }
        }


        private DateTime getDate() 
        {
            DateTime selectedDate;
            if (calendar.SelectedDate.HasValue)
            { selectedDate = calendar.SelectedDate.Value; }
            else
            {
                MessageBox.Show("Please select the date for your shift", "Could not create shift");
                return DateTime.Parse("01/01/1990");
            }
            return selectedDate;
        }


        private string getSelectedDoctor() 
        {
            string doctor;
            if (employeeList.SelectedItems.Count == 1)
                doctor = employeeList.SelectedItems[0].ToString();
            else
            {
                MessageBox.Show("Please select a doctor for your shift", "Could not create shift");
                return null;
            }
            // NO MIDDLE NAME EXAMPLE:   Dr. |Alison  Lee               | represents what string looks like after slicing that happens on line below
            // MIDDLE NAME EXAMPLE   :   Dr. |Alison Madison Lee
            doctor = doctor.Remove(0, 4); //Removes "Dr. "
            string firstName = doctor.Substring(0, doctor.IndexOf(" "));
            string lastName = doctor.Substring(doctor.LastIndexOf(" "));
            doctor = firstName + lastName;

            return doctor;
        }


        private string getShiftStart() 
        {
            if ((hoursComboBox.SelectedIndex == -1) || (minsComboBox.SelectedIndex == -1))
            {
                MessageBox.Show("Please select a valid starting time for your shift", "Could not create shift");
                return null;
            }
            return hoursComboBox.SelectedItem.ToString() + minsComboBox.SelectedItem.ToString();
        }


        private string getShiftEnd() 
        {
            if ((hoursComboBox2.SelectedIndex == -1) || (minsComboBox2.SelectedIndex == -1))
            {
                MessageBox.Show("Please select a valid ending time for your shift", "Could not create shift");
                return null;
            }
            return hoursComboBox2.SelectedItem.ToString() + minsComboBox2.SelectedItem.ToString();
        }


        private string getDoctorUsername(string name, SQLiteConnection conn) 
        {
            string firstName = name.Substring(0, name.IndexOf(" "));
            string lastName = name.Substring(name.LastIndexOf(" ") + 1);
            Console.WriteLine(firstName); Console.WriteLine(lastName) ;
            string sqlQuery = $"SELECT Username FROM Accounts WHERE Firstname = '" + firstName + "'AND Lastname = '" + lastName + "';";
            var cmd = new SQLiteCommand(sqlQuery, conn);
            SQLiteDataReader reader = cmd.ExecuteReader();

            while (reader.Read()) 
            {
                return reader["Username"].ToString();
            }
            return null;
        }


        private void updateShift(object sender, RoutedEventArgs e)
        {
            DateTime shiftDate = getDate();
            if (shiftDate == DateTime.Parse("01/01/1990"))
                return;

            string doctorSelected = getSelectedDoctor();
            if (doctorSelected == null)
                return;


            string shiftStart = getShiftStart();
            string shiftEnd = getShiftEnd();
            
            if ((string.IsNullOrEmpty(shiftStart)) || (string.IsNullOrEmpty(shiftEnd)))
                return;
            if (Int32.Parse(shiftStart) > Int32.Parse(shiftEnd))
            {
                MessageBox.Show("Shift start CANNOT be after shift end", "Could not create shift");
                return;
            }

            String sqlPath = LoadConnectionString();
            SQLiteConnection connection = new SQLiteConnection(sqlPath);
            connection.Open();
            string doctorUsername = getDoctorUsername(doctorSelected, connection);
            connection.Close();

            //Parameters.Add used to prevent SQL injection based attacks.
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.CommandText = @"INSERT INTO Schedule (Day, Username, Shift_Start, Shift_End) VALUES(@Day,@Username,@Shift_Start,@Shift_End)";
            cmd.Parameters.Add(new SQLiteParameter("@Day", shiftDate.ToString()));
            cmd.Parameters.Add(new SQLiteParameter("@Username", doctorUsername));
            cmd.Parameters.Add(new SQLiteParameter("@Shift_Start", shiftStart));
            cmd.Parameters.Add(new SQLiteParameter("@Shift_End", shiftEnd));
            cmd.Connection = connection;

            connection.Open();

            int i = cmd.ExecuteNonQuery();
            if (i == 1) 
            {
                MessageBox.Show("Shift created successfully", "Shift created");
                connection.Close();
            }
            
            
        }
    }
}
