using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;

namespace Appointment_Mgr.Dialog.OTP
{
    public class OTPBoxViewModel : DialogBoxViewModelBase<DialogResults>
    {
        public string _numberBox1 = "", _numberBox2 = "", _numberBox3 = "",
                      _numberBox4 = "", _numberBox5 = "", _numberBox6 = "";
        public ICommand OKCommand { get; private set; }
        public string Title { get; set; }
        public string Message { get; set; }

        public string NumberBox1 
        {
            get { return this._numberBox1; }
            set 
            {
                int isInt;
                if(!string.Equals(this._numberBox1, value))
                {
                    if (Int32.TryParse(value, out isInt))
                    {
                        this._numberBox1 = value;
                    }
                    else 
                    {
                        value = "";
                        return;
                    }
                }
            } 
        }
        public string NumberBox2
        {
            get { return this._numberBox2; }
            set
            {
                int isInt;
                if (!string.Equals(this._numberBox2, value))
                {
                    if (Int32.TryParse(value, out isInt))
                    {
                        this._numberBox2 = value;
                    }
                    else
                    {
                        value = "";
                        return;
                    }
                }
            }
        }
        public string NumberBox3
        {
            get { return this._numberBox3; }
            set
            {
                int isInt;
                if (!string.Equals(this._numberBox3, value))
                {
                    if (Int32.TryParse(value, out isInt))
                    {
                        this._numberBox3 = value;
                    }
                    else
                    {
                        value = "";
                        return;
                    }
                }
            }
        }
        public string NumberBox4
        {
            get { return this._numberBox4; }
            set
            {
                int isInt;
                if (!string.Equals(this._numberBox4, value))
                {
                    if (Int32.TryParse(value, out isInt))
                    {
                        this._numberBox4 = value;
                    }
                    else
                    {
                        value = "";
                        return;
                    }
                }
            }
        }
        public string NumberBox5
        {
            get { return this._numberBox5; }
            set
            {
                int isInt;
                if (!string.Equals(this._numberBox5, value))
                {
                    if (Int32.TryParse(value, out isInt))
                    {
                        this._numberBox5 = value;
                    }
                    else
                    {
                        value = "";
                        return;
                    }
                }
            }
        }
        public string NumberBox6
        {
            get { return this._numberBox6; }
            set
            {
                int isInt;
                if (!string.Equals(this._numberBox6, value))
                {
                    if (Int32.TryParse(value, out isInt))
                    {
                        this._numberBox6 = value;
                    }
                    else
                    {
                        value = "";
                        return;
                    }
                }
            }
        }

        public OTPBoxViewModel(string title, string message) : base(title, message)
        {
            OKCommand = new RelayCommand<IDialogWindow>(OK);
            Title = title;
            Message = message;
            NumberBox1 = _numberBox1; NumberBox2 = _numberBox1; NumberBox3 = _numberBox3;
            NumberBox4 = _numberBox4; NumberBox5 = _numberBox5; NumberBox6 = _numberBox6;
        }

        private void OK(IDialogWindow window)
        {
            string inputtedCode = NumberBox1 + NumberBox2 + NumberBox3 +
                                  NumberBox4 + NumberBox5 + NumberBox6;
            CloseDialogWithResult(window, inputtedCode);
        }
    }
}
