using GalaSoft.MvvmLight.Messaging;
using System;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Schedule_Mgr
{
    /// <summary>
    /// Interaction logic for ResetStaffPasswordWindow.xaml
    /// </summary>
    public partial class ResetStaffPasswordWindow : Window
    {
        public ResetStaffPasswordWindow()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        private SQLiteConnection OpenConnection()
        {
            string sqlPath = LoadConnectionString();
            SQLiteConnection connection = new SQLiteConnection(sqlPath, true);
            connection.Open();

            return connection;
        }

        private static string LoadConnectionString(string id = "Staff")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString.Replace("{AppDir}", AppDomain.CurrentDomain.BaseDirectory);
        }


        private bool Validate_Username(string user)
        {
            string sqlPath = LoadConnectionString();    //Retrieves path from App.config
            SQLiteConnection connection = new SQLiteConnection(sqlPath, true);

            //Can refactor to select user, pass, account type from Accounts WHERE username = args user
            string sqlQuery = $"SELECT Username, Password, Account_Type From Accounts;";

            connection.Open();
            var cmd = new SQLiteCommand(sqlQuery, connection);
            SQLiteDataReader reader = cmd.ExecuteReader();

            bool validUser = false;
            while (reader.Read())
            {
                if (reader["Username"].ToString().Equals(user))
                {
                    validUser = true;
                }
            }
            return validUser;
        }

        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passBox = sender as PasswordBox;

            if (System.Text.RegularExpressions.Regex.IsMatch(passBox.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$"))
            {
                indicator.Text = "very strong";
                indicator.Foreground = new SolidColorBrush(Color.FromRgb(155, 89, 182));
            }
            else if (System.Text.RegularExpressions.Regex.IsMatch(passBox.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$"))
            {
                indicator.Text = "strong";
                indicator.Foreground = new SolidColorBrush(Color.FromRgb(22, 160, 133));
            }
            else
            {
                indicator.Text = "weak";
                indicator.Foreground = new SolidColorBrush(Color.FromRgb(192, 57, 43));
            }
        }

        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            if (NameTextBox.Text.Length == 0 || PassTextBox.Password.Length == 0 || ConfirmPasswordBox.Password.Length == 0)
            {
                MessageBox.Show("Please Fill In Fields!", "All fields must be fulfilled.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!Validate_Username(NameTextBox.Text))
            {
                MessageBox.Show("User Does Not Exist", "Please verify username.", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Validate_Password(PassTextBox.Password)) 
                ResetAccountPassword(NameTextBox.Text, PassTextBox.Password);
            else
                return;

        }


        private bool Validate_Password(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Error! Account could not be created.", "Password is a required field. Please enter a strong password", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (indicator.Text == "weak")
            {
                MessageBox.Show("The inputted password does not meet security requirements.", "Insecure Password", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (PassTextBox.Password.ToString() != ConfirmPasswordBox.Password.ToString()) 
            {
                MessageBox.Show("The passwords inputted do not match.", "Passwords do not match.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;

        }

        private void ResetAccountPassword(string username, string password)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            SQLiteConnection connection = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(@"UPDATE Accounts SET Password=@password WHERE Username=@username", connection);
            cmd.Prepare();
            cmd.Parameters.Add("@username", DbType.String).Value = username;
            cmd.Parameters.Add("@password", DbType.String).Value = hashedPassword;
            cmd.ExecuteNonQuery();
            connection.Close();

            MessageBox.Show("Staff Account Password has been reset.", "Password Reset Successfully", MessageBoxButton.OK, MessageBoxImage.Question);
            this.Close();
        }
    }
}
