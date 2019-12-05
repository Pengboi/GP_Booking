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

namespace Schedule_Mgr
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();

        public MainWindow()
        {
            InitializeComponent();
        }

        private SqlConnection Database_Connect() {
            //THIS SHOULD BE CHANGED TO WHERE DATABASE IS.
            String SQLPath = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Lidio\\iCloudDrive\\Desktop\\" +
                             "Software_Development\\GP_Booking\\Schedule_Mgr\\GP_Booking\\Schedule_Mgr\\EmployeeAccounts.mdf;" +
                             "Integrated Security=True;" +
                             "Connect Timeout=30";
            SqlConnection connection = new SqlConnection(SQLPath);
            return connection;
        }

        private bool Validate_Credentials(String user, String pass) {
            SqlConnection connection = Database_Connect();
            connection.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "select Username, Password, Account_Type from [Table]";
            cmd.Connection = connection;

            Boolean validUser = false, validPass = false;
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (reader["Username"].ToString().Equals(user))
                {
                    validUser = true;
                    if (BCrypt.Net.BCrypt.Verify(pass, reader["Password"].ToString()))
                    {
                        validPass = true;
                        if (!reader["Account_Type"].Equals(3))
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

            return false;
        }

        private void Login_Request(object sender, RoutedEventArgs e) {
            String username = UsernameBox.Text;
            String password = passwordBox.Password;
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
