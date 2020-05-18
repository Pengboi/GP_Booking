using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Appointment_Mgr.Dialog
{
    public class CancellationReasonBoxViewModel : DialogBoxViewModelBase<DialogResults>, INotifyPropertyChanged
    {
        string[] _reasons = { "Tardy", "No Longer Needed", "Other" };
        private string _selectedReason = "Tardy", _specifiedReason;

        // Implementing INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }


        public ICommand SubmitCommand { get; private set; }
        public string Title { get; set; }

        public string[] Reasons { get { return _reasons; } set { value = _reasons; } }

        public string SelectedReason
        {
            get { return _selectedReason; }
            set 
            { 
                SetField(ref _selectedReason, value.Replace("System.Windows.Controls.ComboBoxItem: ", ""), "SelectedReason"); 
            }
            
        }

        public string SpecifiedReason 
        {
            get { return _specifiedReason; }
            set { SetField(ref _specifiedReason, value, "SpecifiedReason"); }
        }

        public CancellationReasonBoxViewModel(string title, string message) : base(title, message)
        {
            SubmitCommand = new RelayCommand<IDialogWindow>(Submit);
            Title = title;
        }

        private void Submit(IDialogWindow window)
        {
            Console.WriteLine("It's bugging me. " + SelectedReason);
            if (SelectedReason == "Other")
            {
                if (string.IsNullOrWhiteSpace(SpecifiedReason))
                    CloseDialogWithResult(window, "No Reason Specified By Receptionist");
                else
                    CloseDialogWithResult(window, "Receptionist: " + SpecifiedReason);
            }
            else
                CloseDialogWithResult(window, SelectedReason);
        }
    }
}
