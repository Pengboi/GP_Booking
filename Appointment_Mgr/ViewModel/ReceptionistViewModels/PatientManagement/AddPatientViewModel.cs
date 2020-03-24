using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Appointment_Mgr.Dialog;
using System.Windows.Input;
using System.Data.SQLite;
using System.Configuration;
using System.Data;

namespace Appointment_Mgr.ViewModel
{
    public class AddPatientViewModel : ViewModelBase
    {

        public string Firstname { get; set; }
        public string Middlename { get; set; }
        public string Lastname { get; set; }
        public string Gender { get; set; }
        public DateTime? DOB { get; set; }
        public string AddressNo { get; set; }
        public string Postcode { get; set; }

        public RelayCommand CreateRecordCommand { private set; get; }


        private SQLiteConnection OpenConnection(string id = "Staff")
        {
            SQLiteConnection connection = new SQLiteConnection(LoadConnectionString(id));
            connection.Open();
            return connection;
        }

        private static string LoadConnectionString(string id) 
        { return ConfigurationManager.ConnectionStrings[id].ConnectionString; }

        
        private IDialogBoxService _dialogService;
        public ICommand AlertCommand { get; private set; }
        public ICommand ErrorCommand { get; private set; }
        public ICommand SuccessCommand { get; private set; }

        //Dialog box definitions
        private string Confirmation(string title, string message) 
        {
            var dialog = new ConfirmationBoxViewModel(title, message);
            var result = _dialogService.OpenDialog(dialog);
            return result;
        }
        private void Alert(string title, string message)
        {
            var dialog = new AlertBoxViewModel(title, message);
            var result = _dialogService.OpenDialog(dialog);
        }
        private void Error(string title, string message)
        {
            var dialog = new Dialog.ErrorBoxViewModel(title, message);
            var result = _dialogService.OpenDialog(dialog);
        }
        private void Success(string title, string message)
        {
            var dialog = new Dialog.SuccessBoxViewModel(title, message);
            var result = _dialogService.OpenDialog(dialog);
        }

        public AddPatientViewModel() 
        {
            _dialogService = new DialogBoxService();

            if (IsInDesignMode)
            {
                Firstname = "Alfred";
                Lastname = "Gjoni";
                Gender = "Male";
                DOB = DateTime.Parse("04/04/1991");
                AddressNo = "24";
                Postcode = "IG12DH";
            }

            CreateRecordCommand = new RelayCommand(AddPatientRecord);
        }

        private void AddPatientRecord()
        {
            if (!VerifyRequiredFields())
                return;

            DateTime dateOfBirth = (DateTime)DOB;
            SQLiteConnection connection = OpenConnection("Patients");

            string cmdString = "";
            if (string.IsNullOrWhiteSpace(Middlename))
                cmdString = @"SELECT COUNT(*) FROM Patient_Data WHERE Firstname = @firstname" +
                            " AND Lastname = @lastname AND DOB = @dob AND Gender = @gender AND ST_Number = @streetNumber AND Postcode = @postcode";
            else
                cmdString = @"SELECT COUNT(*) FROM Patient_Data WHERE Firstname = @firstname AND Middlename = @middlename " +
                            " AND Lastname = @lastname AND DOB = @dob AND Gender = @gender AND ST_Number = @streetNumber AND Postcode = @postcode";

            SQLiteCommand cmd = new SQLiteCommand(cmdString, connection); 
            cmd.Prepare();
            cmd.Parameters.Add("@firstname", DbType.String).Value = Firstname.ToLower(new System.Globalization.CultureInfo("en-UK", false));
            if (!string.IsNullOrWhiteSpace(Middlename))
                cmd.Parameters.Add("@middlename", DbType.String).Value = Middlename.ToLower(new System.Globalization.CultureInfo("en-UK", false));
            cmd.Parameters.Add("@lastname", DbType.String).Value = Lastname.ToLower(new System.Globalization.CultureInfo("en-UK", false));
            cmd.Parameters.Add("@dob", DbType.String).Value = dateOfBirth.ToString("dd/MM/yyyy");
            cmd.Parameters.Add("@gender", DbType.String).Value = Gender;
            cmd.Parameters.Add("@streetNumber", DbType.String).Value = AddressNo;
            cmd.Parameters.Add("@postcode", DbType.String).Value = Postcode.Replace(" ", "").ToUpper();

            //  Checks if record already exists, if it does a prompt is shown to the user,
            //  user decides if duplicate record is required.
            int recordsFound = Convert.ToInt32(cmd.ExecuteScalar());
            if (recordsFound > 0)
            {
                string createDuplicate = Confirmation("Are you sure?",
                    "Patient Record with identical information already exists. Are you sure you want to create another record with this data?");
                if (createDuplicate != "Yes")
                    return;
            }


            // Record added to patient database.

            if (string.IsNullOrWhiteSpace(Middlename))
                cmdString = @"INSERT INTO Patient_Data (Firstname, Lastname, DOB, Gender, ST_Number, Postcode) 
                            VALUES (@firstname, @lastname, @dob, @gender, @streetNumber, @postcode)";
            else
                cmdString = @"INSERT INTO Patient_Data (Firstname, Middlename, Lastname, DOB, Gender, ST_Number, Postcode) 
                            VALUES (@firstname, @middlename, @lastname, @dob, @gender, @streetNumber, @postcode)";

            cmd = new SQLiteCommand(cmdString, connection);
            cmd.Prepare();
            cmd.Parameters.Add("@firstname", DbType.String).Value = Firstname.ToLower(new System.Globalization.CultureInfo("en-UK", false));
            if (!string.IsNullOrEmpty(Middlename))
                cmd.Parameters.Add("@middlename", DbType.String).Value = Middlename.ToLower(new System.Globalization.CultureInfo("en-UK", false));
            cmd.Parameters.Add("@lastname", DbType.String).Value = Lastname.ToLower(new System.Globalization.CultureInfo("en-UK", false));
            cmd.Parameters.Add("@dob", DbType.String).Value = dateOfBirth.ToString("dd/MM/yyyy");
            cmd.Parameters.Add("@gender", DbType.String).Value = Gender;
            cmd.Parameters.Add("@streetNumber", DbType.String).Value = AddressNo;
            cmd.Parameters.Add("@postcode", DbType.String).Value = Postcode.Replace(" ", "").ToUpper();
            cmd.ExecuteNonQuery();

            Success("Success", "Record created. You can now proceed to create an appointment.");
            connection.Close();
        }


        private bool VerifyRequiredFields() 
        {
            if (string.IsNullOrWhiteSpace(Firstname) ||
                string.IsNullOrWhiteSpace(Lastname) ||
                string.IsNullOrWhiteSpace(Gender) ||
                string.IsNullOrWhiteSpace(AddressNo) ||
                string.IsNullOrWhiteSpace(Postcode)) 
            {
                Alert("Required Fields not Complete", "Please complete all required fields in the patient details form." +
                      " If you are not a registered patient, please speak to the receptionist for further assistance.");
                return false;
            }

            //insert code to check address of user using postcode & door number with known-real addresses MoSCoW priority: W
            return true;
        }
    }
}
