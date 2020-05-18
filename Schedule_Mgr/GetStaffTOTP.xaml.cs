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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Schedule_Mgr
{
    /// <summary>
    /// Interaction logic for GetStaffTOTP.xaml
    /// </summary>
    public partial class GetStaffTOTP : Window
    {
        public GetStaffTOTP()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        DataTable dbdataset;

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

        private void searchbox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (searchbox.Text.Equals("Search"))
                searchbox.Clear();
        }

        private void getAccounts(int type)
        {
            SQLiteConnection connection = OpenConnection();
            string sqlQuery = $"SELECT Suffix, Firstname, Middlename, Lastname FROM Accounts WHERE Account_Type = " + type.ToString() + ";";
            var cmd = new SQLiteCommand(sqlQuery, connection);
            SQLiteDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                string middlename = "";
                if (!(reader["Middlename"] == null))
                    middlename = reader["Middlename"].ToString();

                string name = reader["Suffix"].ToString() + " " + reader["Firstname"].ToString() + " " + (!(string.IsNullOrWhiteSpace(middlename)) ? middlename + " " : "") + reader["Lastname"].ToString();
                if (type == 1)
                    receptionistList.Items.Add(name);
                else if (type == 2)
                    doctorsList.Items.Add(name);
                else if (type == 3)
                    HRMList.Items.Add(name);
            }
            reader.Close();
            connection.Close();
        }

        private void filterAccounts(string searchtext)
        {
            string searchRequest = searchtext.Replace(" ", String.Empty).ToLower(new System.Globalization.CultureInfo("en-UK", false));   //Removes whitespace

            SQLiteConnection connection = OpenConnection();
            string sqlQuery = "SELECT Suffix, Firstname, Middlename, Lastname, Account_Type FROM Accounts";
            var cmd = new SQLiteCommand(sqlQuery, connection);

            SQLiteDataAdapter adapter = new SQLiteDataAdapter();
            adapter.SelectCommand = cmd;
            dbdataset = new DataTable();
            adapter.Fill(dbdataset);
            adapter.Update(dbdataset);

            foreach (DataRow row in dbdataset.Rows)
            {
                int accountType = Convert.ToInt32(row["Account_Type"].ToString());
                string middlename = "";
                if (!(row["Middlename"] == null))
                    middlename = row["Middlename"].ToString();
                string name = row["Suffix"].ToString() + " " + row["Firstname"].ToString() + " " + (!(string.IsNullOrWhiteSpace(middlename)) ? middlename + " " : "") + row["Lastname"].ToString();

                string searchResult = name.Replace(" ", String.Empty).ToLower(new System.Globalization.CultureInfo("en-UK", false));
                if (searchResult.Contains(searchRequest))
                {
                    if (accountType == 1)
                        receptionistList.Items.Add(name);
                    else if (accountType == 2)
                        doctorsList.Items.Add(name);
                    else if (accountType == 3)
                        HRMList.Items.Add(name);
                }
            }
            connection.Close();
        }

        private void getTOTP(string fullname)
        {
            SQLiteConnection connection = OpenConnection();
            string cmdString = ""; string middlename = ""; string lastname = "";
            fullname = fullname.Remove(0, fullname.IndexOf(" ") + 1);
            var names = fullname.Split(' ');
            string firstname = names[0];
            if (names.Length == 2)
            {
                lastname = names[1];
                cmdString = @"SELECT COUNT(*) FROM Accounts WHERE Firstname = @fname AND Lastname = @lname";
            }
            else
            {
                middlename = names[1];
                lastname = names[2];
                cmdString = @"SELECT COUNT(*) FROM Accounts WHERE Firstname = @fname AND Middlename = @mname AND Lastname = @lname";
            }

            SQLiteCommand cmd = new SQLiteCommand(cmdString, connection);
            cmd.Prepare();
            cmd.Parameters.Add("@fname", DbType.String).Value = firstname;
            if (names.Length == 3)
                cmd.Parameters.Add("@mname", DbType.String).Value = middlename;
            cmd.Parameters.Add("@lname", DbType.String).Value = lastname;

            int userRecords = Convert.ToInt32(cmd.ExecuteScalar());
            if (userRecords > 1)
            {
                string username = "";
                MessageBox.Show("More than 1 Account was found with these credentials. Please type in the username of the account to confirm.", "Caution! Attention Required!");
                bool foundAccount = false;
                while (!foundAccount)
                {
                    InputDialogBox inputDialog = new InputDialogBox("Enter The Account Username:");
                    if (inputDialog.ShowDialog() == true)
                        username = inputDialog.Answer;
                    cmd = new SQLiteCommand(@"SELECT COUNT(*) FROM Accounts WHERE Username = @user AND Firstname = @fname" +
                        ((!string.IsNullOrEmpty(middlename) ? " AND Middlename = @mname" : "")) + " AND Lastname = @lname", connection);
                    cmd.Prepare();
                    cmd.Parameters.Add("@user", DbType.String).Value = username;
                    cmd.Parameters.Add("@fname", DbType.String).Value = firstname;
                    if (!string.IsNullOrEmpty(middlename))
                        cmd.Parameters.Add("@mname", DbType.String).Value = middlename;
                    cmd.Parameters.Add("@lname", DbType.String).Value = lastname;

                    int userExists = Convert.ToInt32(cmd.ExecuteScalar());
                    if (userExists == 1)
                        foundAccount = true;
                    else
                    {
                        MessageBox.Show("User not found. Confirm username of the account and try again later.", "Error!");
                        return;
                    }

                }
                cmd = new SQLiteCommand(@"SELECT OTP_Token FROM Accounts WHERE Username = @Username", connection);
                cmd.Prepare();
                cmd.Parameters.Add("@Username", DbType.String).Value = username;
                string totpToken = cmd.ExecuteScalar().ToString();
                ShowQRWindow showQR = new ShowQRWindow(totpToken, username);
                showQR.Show();
                this.Close();
            }
            else
            {
                cmdString = cmdString.Replace("COUNT(*)", "OTP_Token, Username");
                cmd = new SQLiteCommand(cmdString, connection);
                cmd.Prepare();
                cmd.Parameters.Add("@fname", DbType.String).Value = firstname;
                if (names.Length == 3)
                    cmd.Parameters.Add("@mname", DbType.String).Value = middlename;
                cmd.Parameters.Add("@lname", DbType.String).Value = lastname;
                SQLiteDataReader reader = cmd.ExecuteReader();

                string totpToken = ""; string username = "";
                while (reader.Read()) 
                {
                    totpToken = reader[0].ToString();
                    username = reader[1].ToString();
                }
                ShowQRWindow showQR = new ShowQRWindow(totpToken, username);
                showQR.Show();
                this.Close();
            }
            connection.Close();
            
        }

        private void receptionistList_Loaded(object sender, RoutedEventArgs e)
        {
            receptionistList.Items.Clear();
            getAccounts(1);
        }

        private void doctorsList_Loaded(object sender, RoutedEventArgs e)
        {
            doctorsList.Items.Clear();
            getAccounts(2);
        }

        private void HRMList_Loaded(object sender, RoutedEventArgs e)
        {
            HRMList.Items.Clear();
            getAccounts(3);
        }

        private void receptionistList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            doctorsList.UnselectAll();
            HRMList.UnselectAll();
        }

        private void doctorsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            receptionistList.UnselectAll();
            HRMList.UnselectAll();
        }

        private void HRMList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            doctorsList.UnselectAll();
            receptionistList.UnselectAll();
        }

        private void searchbox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(searchbox.Text))
            {
                searchbox.Text = "Search";
                receptionistList_Loaded(sender, e);
                doctorsList_Loaded(sender, e);
                HRMList_Loaded(sender, e);
            }
        }

        private void searchbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (searchbox.Text.Equals("Search"))
            {
                return;
            }
            if (string.IsNullOrEmpty(searchbox.Text))
            {
                receptionistList_Loaded(sender, e);
                doctorsList_Loaded(sender, e);
                HRMList_Loaded(sender, e);
            }
            receptionistList.Items.Clear();
            doctorsList.Items.Clear();
            HRMList.Items.Clear();
            filterAccounts(searchbox.Text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (receptionistList.SelectedItems.Count == 0 &&
                doctorsList.SelectedItems.Count == 0 &&
                HRMList.SelectedItems.Count == 0)
            {
                MessageBox.Show("No Account Has Been Selected.", "Could Not Delete Account", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (receptionistList.SelectedItems.Count == 1)
            {
                getTOTP(receptionistList.SelectedItems[0].ToString());
            }
            else if (doctorsList.SelectedItems.Count == 1)
            {
                getTOTP(doctorsList.SelectedItems[0].ToString());
            }
            else if (HRMList.SelectedItems.Count == 1)
            {
                getTOTP(HRMList.SelectedItems[0].ToString());
            }
        }
    }
}
