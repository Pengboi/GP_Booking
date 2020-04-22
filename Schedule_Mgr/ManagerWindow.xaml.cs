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
using GalaSoft.MvvmLight.Messaging;

namespace Schedule_Mgr
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ManagerWindow : UserControl
    {
        public ManagerWindow()
        {
            InitializeComponent();
        }
        public class ShiftRow
        {
            public string DoctorHeader { get; set; }
            public string ShiftStartHeader { get; set; }
            public string ShiftEndHeader { get; set; }
        }

        private static string LoadConnectionString(string id = "Staff")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }


        private SQLiteConnection OpenConnection()
        {
            string sqlPath = LoadConnectionString();
            SQLiteConnection connection = new SQLiteConnection(sqlPath);
            connection.Open();

            return connection;
        }


        private void calendar_CalendarFormat(object sender, RoutedEventArgs e)
        {
            calendar.DisplayDateStart = DateTime.Today;
            calendar.DisplayDateEnd = DateTime.Today.AddDays(14);

            DateTime selectedDay = DateTime.Today;
            DateTime maxDay = DateTime.Today.AddDays(15);

            while (selectedDay != maxDay)
            {
                if ((selectedDay.DayOfWeek == DayOfWeek.Saturday) || (selectedDay.DayOfWeek == DayOfWeek.Sunday))
                {
                    calendar.BlackoutDates.Add(new CalendarDateRange(selectedDay));
                }
                selectedDay = selectedDay.AddDays(1);
            }
        }




        public void doctorList_GetDoctors(object sender, RoutedEventArgs e)
        {
            SQLiteConnection connection = OpenConnection();
            string sqlQuery = $"SELECT Suffix, Firstname, Middlename, Lastname FROM Accounts WHERE Account_Type = 2;";

            var cmd = new SQLiteCommand(sqlQuery, connection);
            SQLiteDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                string middlename = "";
                if (!(reader["Middlename"] == null))
                    middlename = reader["Middlename"].ToString();

                string name = reader["Suffix"].ToString() + " " + reader["Firstname"].ToString() + " " + (!(string.IsNullOrWhiteSpace(middlename)) ? middlename + " " : "") + reader["Lastname"].ToString();
                doctorList.Items.Add(name);
            }
            reader.Close();
            connection.Close();

        }


        private void comboBox_SetTimes(object sender, RoutedEventArgs e)
        {
            for (int i = 6; i <= 19; i++)
            {
                hoursComboBox.Items.Add(i.ToString("00"));
                hoursComboBox2.Items.Add(i.ToString("00"));
            }
            for (int i = 0; i <= 55; i = i + 5)
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
                return DateTime.Parse("01/01/1990");
            }
            return selectedDate;
        }


        private string getSelectedDoctor()
        {
            string doctor;
            if (doctorList.SelectedItems.Count == 1)
                doctor = doctorList.SelectedItems[0].ToString();
            else
            {
                return null;
            }
            doctor = doctor.Remove(0, 4); //Removes "Dr. "
            string firstName = doctor.Substring(0, doctor.IndexOf(" "));
            string lastName = doctor.Substring(doctor.LastIndexOf(" "));
            doctor = firstName + lastName;

            return doctor;
        }

        private string getSelectedShift()
        {
            string doctor;
            if (dailyScheduleGrid.SelectedItems.Count == 1)
            {
                var selectedDoctor = dailyScheduleGrid.SelectedItem as ShiftRow;
                doctor = selectedDoctor.DoctorHeader.ToString();
            }
            else
            {
                return null;
            }
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
            string readerResult = "";
            string sqlQuery = $"SELECT Username FROM Accounts WHERE Firstname = '" + firstName + "'AND Lastname = '" + lastName + "';";
            var cmd = new SQLiteCommand(sqlQuery, conn);
            SQLiteDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                readerResult = reader["Username"].ToString();
            }
            reader.Close();
            return readerResult;
        }


        private bool shiftExists(string date, string username, SQLiteConnection conn)
        {
            SQLiteCommand cmd = new SQLiteCommand(@"SELECT COUNT(*) FROM Schedule WHERE Date = @Date AND Username = @Username", conn);
            cmd.Prepare();
            cmd.Parameters.Add("@Date", DbType.String).Value = date;
            cmd.Parameters.Add("@Username", DbType.String).Value = username;
            int userExists = Convert.ToInt32(cmd.ExecuteScalar());

            if (userExists != 0)
            {
                return true;
            }
            return false;
        }


        private void updateShift(object sender, RoutedEventArgs e)
        {
            DateTime shiftDate = getDate().Date;
            if (shiftDate == DateTime.Parse("01/01/1990"))
            {
                MessageBox.Show("Please select the date for your shift", "Could not create shift");
                return;
            }

            string doctorSelected = getSelectedDoctor();
            if (doctorSelected == null)
            {
                MessageBox.Show("Please select a doctor for your shift", "Could not create shift");
                return;
            }

            string shiftStart = getShiftStart();
            if (string.IsNullOrEmpty(shiftStart))
                return;
            string shiftEnd = getShiftEnd();
            if (string.IsNullOrEmpty(shiftEnd))
                return;

            if (Int32.Parse(shiftStart) > Int32.Parse(shiftEnd))
            {
                MessageBox.Show("Shift start CANNOT be after shift end", "Could not create shift");
                return;
            }

            string sqlPath = LoadConnectionString();
            SQLiteConnection connection = new SQLiteConnection(sqlPath);
            connection.Open();
            string doctorUsername = getDoctorUsername(doctorSelected, connection);
            if (shiftExists(shiftDate.ToString("dd/MM/yyyy"), doctorUsername, connection))
            {
                MessageBox.Show("A shift already exists for this employee on the selected date. Please select another date or delete the existing shift for the employee", "Duplicate shift");
                return;
            }
            //Parameters.Add used to prevent SQL injection based attacks.
            SQLiteCommand cmd = new SQLiteCommand(@"INSERT INTO Schedule (Date, Username, Shift_Start, Shift_End) VALUES (@Date, @Username, @Shift_Start, @Shift_End)", connection);
            cmd.Prepare();
            cmd.Parameters.Add("@Date", DbType.String).Value = shiftDate.ToString("dd/MM/yyyy");
            cmd.Parameters.Add("@Username", DbType.String).Value = doctorUsername;
            cmd.Parameters.Add("@Shift_Start", DbType.String).Value = shiftStart;
            cmd.Parameters.Add("@Shift_End", DbType.String).Value = shiftEnd;

            cmd.ExecuteNonQuery();
            MessageBox.Show("Shift created successfully", "Shift created");
            connection.Close();

            updateDailySchedule();
        }


        private void deleteShift(object sender, RoutedEventArgs e)
        {
            DateTime shiftDate = getDate().Date;
            if (shiftDate == DateTime.Parse("01/01/1990"))
            {
                MessageBox.Show("Please select the date of the shift you want to delete", "Could not delete shift");
                return;
            }
            string doctorSelected = getSelectedShift();
            if (doctorSelected == null)
            {
                MessageBox.Show("Please select the shift you want to delete.", "Could not delete shift");
                return;
            }

            SQLiteConnection connection = OpenConnection();
            string doctorUsername = getDoctorUsername(doctorSelected, connection);
            if (shiftExists(shiftDate.ToString("dd/MM/yyyy"), doctorUsername, connection))
            {
                SQLiteCommand cmd = new SQLiteCommand(@"DELETE FROM Schedule WHERE Date = @Date AND Username = @Username", connection);
                cmd.Prepare();
                cmd.Parameters.Add("@Date", DbType.String).Value = shiftDate.ToString("dd/MM/yyyy");
                cmd.Parameters.Add("@Username", DbType.String).Value = doctorUsername;

                cmd.ExecuteNonQuery();
                MessageBox.Show("Shift for Dr." + doctorSelected + " scheduled on: " + shiftDate.ToString("dd/MM/yyyy") + " deleted successfully", "Shift deleted");
            }
            else
                MessageBox.Show("Shift does not exist", "Could not delete shift");
            connection.Close();

            updateDailySchedule();
            return;
        }

        private void createAccount(object sender, RoutedEventArgs e)
        {
            NewUser NewUserWin = new NewUser();
            NewUserWin.ShowDialog();
            doctorList.DataContext = null;
            doctorList.Items.Clear();
            doctorList_GetDoctors(sender, e);
            return;
        }


        private void deleteAccount(object sender, RoutedEventArgs e)
        {
            DeleteEmployeeWindow DelEmployeeWin = new DeleteEmployeeWindow();
            DelEmployeeWin.ShowDialog();
            doctorList.DataContext = null;
            doctorList.Items.Clear();
            doctorList_GetDoctors(sender, e);
            return;
        }


        private void logOut(object sender, RoutedEventArgs e)
        {
            Messenger.Default.Send<string>("HomeView");
        }


        private string GetDoctorNameByID(string username, SQLiteConnection connection)
        {
            string doctorName = "";
            SQLiteCommand cmd = new SQLiteCommand($"SELECT Suffix, Firstname, Middlename, Lastname FROM Accounts WHERE Username = @Username", connection);
            cmd.Prepare();
            cmd.Parameters.Add("@Username", DbType.String).Value = username;
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string middlename = "";
                if (!(reader["Middlename"] == null))
                    middlename = reader["Middlename"].ToString();

                doctorName = reader["Suffix"].ToString() + " " + reader["Firstname"].ToString() + " " + (!(string.IsNullOrWhiteSpace(middlename)) ? middlename + " " : "") + reader["Lastname"].ToString();
            }
            return doctorName;
        }

        private void calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            updateDailySchedule();
        }

        private void updateDailySchedule()
        {
            dailyScheduleGrid.Items.Clear();
            DateTime selectedDate = getDate().Date;

            SQLiteConnection connection = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(@"SELECT Username, Shift_Start, Shift_End FROM Schedule WHERE Date = @Date", connection);
            cmd.Prepare();
            cmd.Parameters.Add("@Date", DbType.String).Value = selectedDate.ToString("dd/MM/yyyy");

            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string doctorName = GetDoctorNameByID(reader["Username"].ToString(), connection);
                string shiftStart = reader["Shift_Start"].ToString().Insert(2, ":");
                string shiftEnd = reader["Shift_End"].ToString().Insert(2, ":");
                var doctorShiftRow = new ShiftRow { DoctorHeader = doctorName, ShiftStartHeader = shiftStart, ShiftEndHeader = shiftEnd };
                dailyScheduleGrid.Items.Add(doctorShiftRow);
            }
            reader.Close();
            connection.Close();
            return;
        }

        private void showTOTPButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
