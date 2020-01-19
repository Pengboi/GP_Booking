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
    public class OptionalFieldTextOnlyRule : ValidationRule
    {

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var str = value as string;

            // if not a match --> Taken from JavaScript CW. Basically, read error
            if (!Regex.IsMatch(str, @"^(([A-Z]|[a-z]){1}(([a-z])*([-]{1}[A-Za-z]{1})|([-]{0}[A-Z]{0}))[a-z]*$)|(^$)"))
                return new ValidationResult(false, String.Format("Name must contain letters with optional appropriate use of '-' only"));

            return new ValidationResult(true, null);

        }
    }
}
