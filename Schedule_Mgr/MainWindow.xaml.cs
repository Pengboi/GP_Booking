using System;
using System.Windows;
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

namespace Schedule_Mgr
{
   
    public partial class MainWindow : Window
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();
        

        public MainWindow()
        {
            InitializeComponent();
        }

        private static string LoadConnectionString(string id = "Default") 
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }

        private bool Validate_Credentials(String user, String pass) {
            String SQLPath = LoadConnectionString();    //Retrieves path from App.config
            SQLiteConnection connection = new SQLiteConnection(SQLPath);
            string sqlQuery = $"SELECT Username, Password, Account_Type From Accounts;";

            connection.Open();
            var cmd = new SQLiteCommand(sqlQuery, connection);
            SQLiteDataReader reader = cmd.ExecuteReader();

            Boolean validUser = false, validPass = false;
            while (reader.Read())
            {
                if (reader["Username"].ToString().Equals(user))
                {
                    validUser = true;
                    if (BCrypt.Net.BCrypt.Verify(pass, reader["Password"].ToString()))
                    {
                        validPass = true;
                        if (reader.GetInt32(reader.GetOrdinal("Account_Type")) != 3)
                        {
                            MessageBox.Show("Access denied. Your account does not have permission to access this application.", "Unauthorised User");
                            break;
                        }
                        connection.Close();
                        return true;
                    }
                }
            }
            if (!validUser)
                MessageBox.Show("The account could not be found. Try again. If issues persist, contact the IT administrator", "User not found.");
            else if (!validPass)
                MessageBox.Show("The password is incorrect. Try again. If issues persist, contact the IT administrator", "Incorrect password.");

            connection.Close();

            return false;
        }

        private void Login_Request(object sender, RoutedEventArgs e) {
            String username = UsernameBox.Text;
            String password = passwordBox.Password;

            Console.WriteLine(BCrypt.Net.BCrypt.HashPassword(password));

            if (Validate_Credentials(username, password)) 
            {
                MessageBox.Show("*Opens meme*");
            }

            // Memes basically.
            mediaPlayer.Open(new Uri("C:\\Users\\Lidio\\iCloudDrive\\Desktop\\Software_Development\\GP_Booking\\Schedule_Mgr\\GP_Booking\\Schedule_Mgr\\Assets\\stop.mp3"));
            mediaPlayer.Play();

        }

    }
}
