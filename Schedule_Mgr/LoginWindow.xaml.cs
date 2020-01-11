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
   
    public partial class LoginWindow : Window
    {
        
        public LoginWindow()
        {
            InitializeComponent();
        }

        private static string LoadConnectionString(string id = "Default") 
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }

        private bool Validate_Credentials(String user, String pass) {
            String sqlPath = LoadConnectionString();    //Retrieves path from App.config
            SQLiteConnection connection = new SQLiteConnection(sqlPath);

            //Can refactor to select user, pass, account type from Accounts WHERE username = args user ---> check other code behinds on how to
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
            
            reader.Close();
            connection.Close();

            return false;
        }


        private String Get_OTPKEY(String user) 
        {
            String sqlPath = LoadConnectionString();    //Retrieves path from App.config
            SQLiteConnection connection = new SQLiteConnection(sqlPath);
            string sqlQuery = $"SELECT OTP_Token FROM Accounts WHERE Username =  \"" + user + "\";";
            connection.Open();

            var cmd = new SQLiteCommand(sqlQuery, connection);
            String secretKey = cmd.ExecuteScalar().ToString();
            connection.Close();
            return secretKey;
        }


        private bool Validate_OTP(String inputOTP, String user) 
        {
            String storedKey = Get_OTPKEY(user);

            var bytes = Base32Encoding.ToBytes(storedKey);
            var totp = new Totp(bytes);
            var totpCode = totp.ComputeTotp();

            if (inputOTP == totpCode)
                return true;
            else
                MessageBox.Show("Incorrect code was entered. Try again, if issues persist - contact the IT administrator.", "Incorrect code.");
            passwordBox.Clear();
            return false;
        }
        

        private void Login_Request(object sender, RoutedEventArgs e) {
            String username = UsernameBox.Text;
            String password = passwordBox.Password;


            if (Validate_Credentials(username, password)) 
            {
                String inputOTP = "";
                InputDialogBox inputDialog = new InputDialogBox("Enter Your OTP Code:", 6);
                if (inputDialog.ShowDialog() == true)
                    inputOTP = inputDialog.Answer;

                if (Validate_OTP(inputOTP, username)) 
                {
                    ManagerWindow managerWin = new ManagerWindow();
                    managerWin.Show();
                    this.Hide();
                    this.Close();

                }
            }
        }
    }
}
