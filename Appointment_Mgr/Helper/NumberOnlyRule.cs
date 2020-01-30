using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Appointment_Mgr.Helper
{
    public class NumberOnlyRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var str = value as string;
            if (str == null || str == "Required*" || string.IsNullOrWhiteSpace(str))
            {
                return new ValidationResult(false, "Please enter some text");
            }
            // if not a match
            if (!Regex.IsMatch(str, @"^[0-9]*$"))
                return new ValidationResult(false, String.Format("Name must contain letters (may appropriate use '-')"));

            return new ValidationResult(true, null);

        }
    }
}
