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
    public class ValidEmailRule : ValidationRule
    {

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var str = value as string;
            if (str == null || str == "Required*" || string.IsNullOrWhiteSpace(str))
            {
                return new ValidationResult(false, "Please enter your email");
            }
            // if not a match
            if (!Regex.IsMatch(str, @"[\w-_.]*[@]{1}([\w]+[.][\w]+)+$"))
                return new ValidationResult(false, String.Format("Please enter a valid email address"));

            return new ValidationResult(true, null);

        }
    }
}
