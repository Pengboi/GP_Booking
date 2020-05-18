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
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Windows.Shapes;
using OtpNet;

namespace Schedule_Mgr
{
    /// <summary>
    /// Interaction logic for NewUser.xaml
    /// </summary>
    public partial class NewUser : Window
    {
        public NewUser()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        private static string LoadConnectionString(string id = "Staff")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString.Replace("{AppDir}", AppDomain.CurrentDomain.BaseDirectory);
        }

        private SQLiteConnection OpenConnection()
        {
            string sqlPath = LoadConnectionString();
            SQLiteConnection connection = new SQLiteConnection(sqlPath, true);
            connection.Open();

            return connection;
        }

        private string getHonorific() 
        {
            string honorific = "";
            if ((bool)MrHonorific.IsChecked)
                honorific = MrHonorific.Content.ToString();
            if ((bool)MissHonorific.IsChecked)
                honorific = MissHonorific.Content.ToString();
            if ((bool)MrsHonorific.IsChecked)
                honorific = MrsHonorific.Content.ToString();
            if ((bool)DrHonorific.IsChecked)
                honorific = DrHonorific.Content.ToString();
            honorific = honorific.Substring(honorific.LastIndexOf(" ") + 1);
            return honorific;
        }


        private int? getGender() 
        {
            if ((bool)Female.IsChecked)
                return 0; // female
            else
                return 1; // male
        }


        private int? getAccountType() 
        {
            if (string.IsNullOrEmpty(accountTypeBox.Text))
                return -1;
            string userAccountType = accountTypeBox.SelectedItem.ToString();
            userAccountType = userAccountType.Substring(userAccountType.LastIndexOf(" ") + 1);
            if (userAccountType.Equals("Receptionist"))
                return 1;
            else if (userAccountType.Equals("Doctor"))
                return 2;
            else if (userAccountType.Equals("Administrator"))
                return 3;
            return null;
        }

        private string getFirstname() 
        {
            return firstnameBox.Text;
        }

        private string getMiddlename() 
        {
            return middleNameBox.Text;
        }

        private string getLastname() 
        {
            return lastnameBox.Text;
        }

        private string getPassword() 
        {
            string pass = passwordBox.Password;
            string pass2 = passwordBox2.Password;

            if (pass.Equals(null))
                return "null";
            if (!(pass.Equals(pass2))) 
                return "nomatch";
            if (indicator.Text.Equals("weak"))
                return "weak";
            return pass;
        }

        private void Female_Checked(object sender, RoutedEventArgs e)
        {
            if (accountTypeBox == null)
            {
                MrHonorific.Visibility = Visibility.Visible;
                MissHonorific.Visibility = Visibility.Collapsed;
                MrsHonorific.Visibility = Visibility.Collapsed;
                MrHonorific.IsChecked = true;
                return;
            }

            string accountType = accountTypeBox.SelectedItem.ToString();
            accountType = accountType.Substring(accountType.LastIndexOf(" ") + 1);
            if (accountType != "Doctor") 
            {
                MrHonorific.Visibility = Visibility.Collapsed;
                MissHonorific.Visibility = Visibility.Visible;
                MrsHonorific.Visibility = Visibility.Visible;
                MissHonorific.IsChecked = true;
            }
        }

        private void Male_Checked(object sender, RoutedEventArgs e)
        {
            if (accountTypeBox == null)
            {
                MrHonorific.Visibility = Visibility.Visible;
                MissHonorific.Visibility = Visibility.Collapsed;
                MrsHonorific.Visibility = Visibility.Collapsed;
                MrHonorific.IsChecked = true;
                return;
            }

            string accountType = accountTypeBox.SelectedItem.ToString();
            accountType = accountType.Substring(accountType.LastIndexOf(" ") + 1);
            if (accountType != "Doctor") 
            {
                MrHonorific.Visibility = Visibility.Visible;
                MissHonorific.Visibility = Visibility.Collapsed;
                MrsHonorific.Visibility = Visibility.Collapsed;
                MrHonorific.IsChecked = true;
            }
        }

        private void AccountType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selection = accountTypeBox.SelectedItem.ToString();
            selection = selection.Substring(selection.LastIndexOf(" ") + 1);
            if (selection == "Doctor")
            {
                DrHonorific.IsChecked = true;
                MrHonorific.Visibility = Visibility.Collapsed;
                MissHonorific.Visibility = Visibility.Collapsed;
                MrsHonorific.Visibility = Visibility.Collapsed;
            }
            else 
            {
                if (getGender() == 0)
                {
                    MrHonorific.Visibility = Visibility.Collapsed;
                    MissHonorific.Visibility = Visibility.Visible;
                    MrsHonorific.Visibility = Visibility.Visible;
                    MissHonorific.IsChecked = true;
                }
                else 
                {
                    MrHonorific.Visibility = Visibility.Visible;
                    MissHonorific.Visibility = Visibility.Collapsed;
                    MrsHonorific.Visibility = Visibility.Collapsed;
                    MrHonorific.IsChecked = true;
                }
            }
            return;
        }


        private void nameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tBox = sender as TextBox;
            //  You made this from your JavaScript class lol. How funny, huh.
            if (!System.Text.RegularExpressions.Regex.IsMatch(tBox.Text, "^([A-Z]|[a-z]){1}(([a-z])*([-]{1}[A-Za-z]{1})|([-]{0}[A-Z]{0}))[a-z]*$"))
            {
                if (!(string.IsNullOrEmpty(tBox.Text))) 
                    MessageBox.Show("This textbox accepts only alphabetical characters. Make sure only alphabetical characters are used and capital letters are in the correct order.");
                tBox.Text = "";
            }
        }


        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passBox = sender as PasswordBox;

            if (System.Text.RegularExpressions.Regex.IsMatch(passBox.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$"))
            {
                indicator.Text = "very strong";
                indicator.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(155, 89, 182));
            }
            else if (System.Text.RegularExpressions.Regex.IsMatch(passBox.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$"))
            {
                indicator.Text = "strong";
                indicator.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(22, 160, 133));
            }
            else 
            {
                indicator.Text = "weak";
                indicator.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(192, 57, 43));
            }
        }

        private bool isUniqueUsername(string name) 
        {
            SQLiteConnection connection = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(@"SELECT COUNT(*) FROM Accounts WHERE Username = @Username", connection);
            cmd.Prepare();
            cmd.Parameters.Add("@Username", DbType.String).Value = name;

            int userExists = Convert.ToInt32(cmd.ExecuteScalar());
            if (userExists == 0)
            {
                connection.Close();
                return true;
            }
            connection.Close();
            return false;
        }

        private string createUsername(string first, string middle, string last) 
        {
            string username = "";
            // If first is longer than 3 characters, only first 3 letters of first name are used for the username.
            if (first.Length > 3)
                first = first.Substring(0, 3);
            
            if (string.IsNullOrEmpty(middle))
                username = first + last;
            else
                username = first + middle.Substring(0) + last;

            while (!isUniqueUsername(username)) 
            {
                Random rnd = new Random();
                string rndString = rnd.Next(0, 1000).ToString();
                username = username + rndString;
            }
            return username;
        }

        private void addRecordToDB(string user, string pass, string otp, string gender, string honorific, 
            string fname, string mname, string lname, int accountType) 
        {
            SQLiteConnection connection = OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(@"INSERT INTO Accounts (Username, Password, OTP_Token, Gender, Suffix, Firstname, Middlename, Lastname, Account_Type)
                                VALUES (@user, @pass, @otp, @gender, @suffix, @firstname, @middlename, @lastname, @Account_Type)", connection);
            cmd.Prepare();
            cmd.Parameters.Add("@user", DbType.String).Value = user;
            cmd.Parameters.Add("@pass", DbType.String).Value = pass;
            cmd.Parameters.Add("@otp", DbType.String).Value = otp;
            cmd.Parameters.Add("@gender", DbType.String).Value = gender;
            cmd.Parameters.Add("@suffix", DbType.String).Value = honorific;
            cmd.Parameters.Add("@firstname", DbType.String).Value = fname;
            if (string.IsNullOrEmpty(mname))
                cmd.Parameters.Add("@middlename", DbType.String).Value = null;
            else
                cmd.Parameters.Add("@middlename", DbType.String).Value = mname;
            cmd.Parameters.Add("@lastname", DbType.String).Value = lname;
            cmd.Parameters.Add("@Account_Type", DbType.String).Value = accountType;

            cmd.ExecuteNonQuery();
            connection.Close();
        }

        private void createAccountButton_Click(object sender, RoutedEventArgs e)
        {
            string userHonorific = getHonorific();
            if (userHonorific.Equals("null")) 
            {
                MessageBox.Show("Title is a required field. Please select a Title", "Error! Account could not be created.");
                return;
            }
            int? userGender = getGender();
            if (userGender == -1) 
            {
                MessageBox.Show("Gender is a required field. Please select a Gender", "Error! Account could not be created.");
                return;
            }
            int? userAccountType = getAccountType();
            if (userAccountType == -1) 
            {
                MessageBox.Show("Account Type is a required field. Please select an Account Type", "Error! Account could not be created.");
                return;
            }
            string firstname = getFirstname();
            if (string.IsNullOrEmpty(firstname)) 
            {
                MessageBox.Show("First name is a required field. Please enter your first name", "Error! Account could not be created.");
                return;
            }
            string middlename = getMiddlename();
            string lastname = getLastname();
            if (string.IsNullOrEmpty(lastname)) 
            {
                MessageBox.Show("Last name is a required field. Please enter your last name", "Error! Account could not be created.");
                return;
            }
            string password = getPassword();
            if (password.Equals("nomatch")) 
            {
                MessageBox.Show("Passwords do not match.", "Error! Account could not be created.");
                passwordBox.Clear(); passwordBox2.Clear();
                return;
            }
            if (password.Equals("weak")) 
            {
                MessageBox.Show("Cannot accept weak Password. Try using numbers and symbols to add complexity.", "Error! Account could not be created.");
                passwordBox.Clear(); passwordBox2.Clear();
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Password is a required field. Please enter a strong password", "Error! Account could not be created.");
                passwordBox2.Clear();
                return;
            }

            // will not exit function until unique username confirmed.
            string username = createUsername(firstname, middlename, lastname);
            MessageBox.Show("Account Username: " + username, "Account Successfully Created!", MessageBoxButton.OK, MessageBoxImage.Information);
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            //FORMAT FOR QRCODE
            //Add video for how to add 2otp to personal device.
            //              COMPANY:USERNAME?SECRETKEY&issuer=COMPANY
            //otpauth://totp/Meme:arlidio?secret=2FO4X6236KEHZKN4XGVIOH2MCCBDGLZF&issuer=Meme
            var otpKey = KeyGeneration.GenerateRandomKey(20);
            var otpKeyString = Base32Encoding.ToString(otpKey);
            var otpKeyBytes = Base32Encoding.ToBytes(otpKeyString);

            int accountType = Convert.ToInt32(userAccountType);
            string gender = userGender == 0 ? "Female" : "Male"; 

            addRecordToDB(username, hashedPassword, otpKeyString, gender, userHonorific, 
                firstname, middlename, lastname, accountType);

            
            //Probably need another window to create and show the QR code & the video with instructions on how to 
            //add TOTP to app. ----- add past this point
            ShowQRWindow ShowQRWindow = new ShowQRWindow(otpKeyString, username);
            this.Hide();
            this.Close();
            ShowQRWindow.ShowDialog();
            
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
