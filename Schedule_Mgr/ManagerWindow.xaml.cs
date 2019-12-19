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


        private SQLiteConnection startConnection() 
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


        public void employeeList_GetDoctors(object sender, RoutedEventArgs e) 
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
            if (employeeList.SelectedItems.Count == 1)
                doctor = employeeList.SelectedItems[0].ToString();
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
        }


        private void deleteShift(object sender, RoutedEventArgs e) 
        {
            DateTime shiftDate = getDate().Date;
            if (shiftDate == DateTime.Parse("01/01/1990"))
            {
                MessageBox.Show("Please select the date of the shift you want to delete", "Could not delete shift");
                return;
            }
            string doctorSelected = getSelectedDoctor();
            if (doctorSelected == null)
            {
                MessageBox.Show("Please select the doctor whose shift you want to delete.", "Could not delete shift");
                return;
            }

            SQLiteConnection connection = startConnection();
            string doctorUsername = getDoctorUsername(doctorSelected, connection);
            if (shiftExists(shiftDate.ToString("dd/MM/yyyy"), doctorUsername, connection))
            {
                SQLiteCommand cmd = new SQLiteCommand(@"DELETE FROM Schedule WHERE Date = @Date AND Username = @Username", connection);
                cmd.Prepare();
                cmd.Parameters.Add("@Date", DbType.String).Value = shiftDate.ToString("dd/MM/yyyy");
                cmd.Parameters.Add("@Username", DbType.String).Value = doctorUsername;

                cmd.ExecuteNonQuery();
                MessageBox.Show("Shift for Dr." + doctorSelected + " scheduled on: "+ shiftDate.ToString("dd/MM/yyyy") + " deleted successfully", "Shift deleted");
                connection.Close();
            }
            else
                MessageBox.Show("Shift does not exist", "Could not delete shift");
            return;
        }

        private void createAccount(object sender, RoutedEventArgs e) 
        {
            NewUser NewUserWin = new NewUser();
            NewUserWin.ShowDialog();
            employeeList.DataContext = null;
            employeeList.Items.Clear();
            employeeList_GetDoctors(sender, e);
        }


        private void deleteAccount(object sender, RoutedEventArgs e) 
        {
            if (getSelectedDoctor() == null) 
            {
                MessageBox.Show("Select the doctor whose profile you wish to delete.", "No Doctor Selected");
                return;
            }
            
            SQLiteConnection connection = startConnection();
            string doctorUsername = getDoctorUsername(getSelectedDoctor(), connection);
            MessageBoxResult messageBoxConfirmation = System.Windows.MessageBox.Show("Are you sure you would like to delete Dr. " + getSelectedDoctor() + "'s profile?",
                                                                                     "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxConfirmation == MessageBoxResult.No)
                return;
            else
            {
                SQLiteCommand cmd = new SQLiteCommand(@"DELETE FROM Accounts WHERE Username = @Username", connection);
                cmd.Prepare();
                cmd.Parameters.Add("@Username", DbType.String).Value = doctorUsername;
                cmd.ExecuteNonQuery();
                connection.Close();

                employeeList.DataContext = null;
                employeeList.Items.Clear();

                employeeList_GetDoctors(sender, e);
                MessageBox.Show("Account Deleted successfully", "Account Deleted.");
            }
            return;
        }


        private void logOut(object sender, RoutedEventArgs e) 
        {
            LoginWindow loginWin = new LoginWindow();
            this.Hide();
            this.Close();
            loginWin.ShowDialog();
        }
    }
}
