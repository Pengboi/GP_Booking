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
        public ICommand ConfirmationCommand { get; private set; }

        //Dialog box definitions
        private void Alert(string title, string message)
        {
            var dialog = new AlertBoxViewModel(title, message);
            var result = _dialogService.OpenDialog(dialog);
        }
        private void Error(string title, string message)
        {
            var dialog = new Dialog.Error.ErrorBoxViewModel(title, message);
            var result = _dialogService.OpenDialog(dialog);
        }
        private void Confirmation(string title, string message)
        {
            var dialog = new Dialog.Confirmation.ConfirmationBoxViewModel(title, message);
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
            else 
            {

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
            cmd.Parameters.Add("@firstname", DbType.String).Value = Firstname.ToLower();
            if (!string.IsNullOrWhiteSpace(Middlename))
                cmd.Parameters.Add("@middlename", DbType.String).Value = Middlename.ToLower();
            cmd.Parameters.Add("@lastname", DbType.String).Value = Lastname.ToLower();
            cmd.Parameters.Add("@dob", DbType.String).Value = dateOfBirth.ToString("dd/MM/yyyy");
            cmd.Parameters.Add("@gender", DbType.String).Value = Gender;
            cmd.Parameters.Add("@streetNumber", DbType.String).Value = AddressNo;
            cmd.Parameters.Add("@postcode", DbType.String).Value = Postcode.Replace(" ", "");

            //  Checks if record already exists
            int recordsFound = Convert.ToInt32(cmd.ExecuteScalar());
            if (recordsFound == 1)
            {
                Error("Record Already Exists.", "Patient Record already exists. Please verify patient details, if issues persist contact support.");
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
            cmd.Parameters.Add("@firstname", DbType.String).Value = Firstname.ToLower();
            if (!string.IsNullOrEmpty(Middlename))
                cmd.Parameters.Add("@middlename", DbType.String).Value = Middlename.ToLower();
            cmd.Parameters.Add("@lastname", DbType.String).Value = Lastname.ToLower();
            cmd.Parameters.Add("@dob", DbType.String).Value = dateOfBirth.ToString("dd/MM/yyyy");
            cmd.Parameters.Add("@gender", DbType.String).Value = Gender;
            cmd.Parameters.Add("@streetNumber", DbType.String).Value = AddressNo;
            cmd.Parameters.Add("@postcode", DbType.String).Value = Postcode.Replace(" ", "");
            cmd.ExecuteNonQuery();

            Confirmation("Success", "Record created. You can now proceed to create an appointment.");
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
