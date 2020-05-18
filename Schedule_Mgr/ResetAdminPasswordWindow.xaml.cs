using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Schedule_Mgr
{
    /// <summary>
    /// Interaction logic for ResetPasswordWindow.xaml
    /// </summary>
    public partial class ResetAdminPasswordWindow : UserControl
    {
        public string OverrideTokenHash = "$2b$10$thpYggQwNRBx/PNc9y3EO.pkL7GoXbSfkFKkkUlMEKbVaJcMG..we"; //cOnARtrADYaN

        public ResetAdminPasswordWindow()
        {
            InitializeComponent();
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

        private void submitButton_Click(object sender, RoutedEventArgs e) { 
            if (NameTextBox.Text.Length == 0 || PassTextBox.Password.Length == 0 || KeyTextBox.Password.Length == 0) 
            {
                MessageBox.Show("Please Fill In Fields!", "All fields must be fulfilled.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!Validate_Username(NameTextBox.Text)) 
            {
                MessageBox.Show("User Does Not Exist", "Please verify username.", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!Validate_Password(PassTextBox.Password))
                return;

            if (Validate_Token(KeyTextBox.Password))
            {
                ResetAccountPassword(NameTextBox.Text, PassTextBox.Password);
            }
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
                MessageBox.Show("The inputted password does not meet security requirements.", "Insecure Password" , MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
            
        }


        private bool Validate_Token(string token)
        {
            if (BCrypt.Net.BCrypt.Verify(token, OverrideTokenHash))
                return true;
            else 
            {
                MessageBox.Show("Incorrect Token.", "Incorrect Token.", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
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

            MessageBox.Show("Password has been reset. You may now sign-in." , "Password Reset Successfully", MessageBoxButton.OK, MessageBoxImage.Question);
            Messenger.Default.Send<string>("HomeView");
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            Messenger.Default.Send<string>("HomeView");
        }
    }
}
