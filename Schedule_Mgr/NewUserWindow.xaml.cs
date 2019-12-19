using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Drawing;
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

        private string getHonorific() 
        {
            if (string.IsNullOrEmpty(honorificComboBox.Text))
                return "null";
            string honorific = honorificComboBox.SelectedItem.ToString();
            honorific = honorific.Substring(honorific.LastIndexOf(" ") + 1);
            return honorific;
        }


        private int? getGender() 
        {
            if (string.IsNullOrEmpty(genderComboBox.Text))
                return -1;
            string userGender = genderComboBox.SelectedItem.ToString();
            userGender = userGender.Substring(userGender.LastIndexOf(" ") + 1);
            if (userGender.Equals("Female"))
                return 0;
            else if (userGender.Equals("Male"))
                return 1;
            return null;
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
            else if (userAccountType.Equals("HR and Management"))
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

        private void AccountType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selection = accountTypeBox.SelectedItem.ToString();
            selection = selection.Substring(selection.LastIndexOf(" ") + 1);
            if (selection == "Doctor")
            {
                honorificComboBox.Items.Clear();
                honorificComboBox.Items.Add("Dr.");
            }
            else 
            {
                honorificComboBox.Items.Clear();
                honorificComboBox.Items.Add("Mr.");
                honorificComboBox.Items.Add("Miss.");
                honorificComboBox.Items.Add("Mrs.");
                honorificComboBox.Items.Add("Dr.");
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
            SQLiteConnection connection = startConnection();
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

        private void addRecordToDB(string user, string pass, string otp, string honorific, 
            string fname, string mname, string lname, int accountType) 
        {
            SQLiteConnection connection = startConnection();
            SQLiteCommand cmd = new SQLiteCommand(@"INSERT INTO Accounts (Username, Password, OTP_Token, Suffix, Firstname, Middlename, Lastname, Account_Type)
                                VALUES (@user, @pass, @otp, @suffix, @firstname, @middlename, @lastname, @Account_Type)", connection);
            cmd.Prepare();
            cmd.Parameters.Add("@user", DbType.String).Value = user;
            cmd.Parameters.Add("@pass", DbType.String).Value = pass;
            cmd.Parameters.Add("@otp", DbType.String).Value = otp;
            cmd.Parameters.Add("@suffix", DbType.String).Value = honorific;
            cmd.Parameters.Add("@firstname", DbType.String).Value = fname;
            cmd.Parameters.Add("@middlename", DbType.String).Value = mname;
            cmd.Parameters.Add("@lastname", DbType.String).Value = lname;
            cmd.Parameters.Add("@Account_Type", DbType.String).Value = accountType;

            cmd.ExecuteNonQuery();
            MessageBox.Show("Account Created Successfully", "Account created");
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
            MessageBox.Show(username); // REMOVE POST-DEBUG.
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            //FORMAT FOR QRCODE
            //Add video for how to add 2otp to personal device.
            //              COMPANY:USERNAME?SECRETKEY&issuer=COMPANY
            //otpauth://totp/Meme:arlidio?secret=2FO4X6236KEHZKN4XGVIOH2MCCBDGLZF&issuer=Meme
            var otpKey = KeyGeneration.GenerateRandomKey(20);
            var otpKeyString = Base32Encoding.ToString(otpKey);
            var otpKeyBytes = Base32Encoding.ToBytes(otpKeyString);
            Console.WriteLine(otpKeyString); //This should be stored in DB

            int accountType = Convert.ToInt32(userAccountType);

            addRecordToDB(username, hashedPassword, otpKeyString, userHonorific, 
                firstname, middlename, lastname, accountType);

            
            //Probably need another window to create and show the QR code & the video with instructions on how to 
            //add TOTP to app. ----- add past this point
            showQRCode(otpKeyString, username);
        }

        private void showQRCode(string key, string username) 
        {
            string qrString = "otpauth://totp/GP:" + username + "?secret=" + key + "&issuer=GP"; 
            QRCoder.QRCodeGenerator qrGenerator = new QRCoder.QRCodeGenerator();
            QRCoder.QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrString, QRCoder.QRCodeGenerator.ECCLevel.Q);
            QRCoder.QRCode qrCode = new QRCoder.QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
