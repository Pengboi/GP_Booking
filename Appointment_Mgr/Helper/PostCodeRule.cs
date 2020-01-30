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
    public class PostcodeRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var str = value as string;
            if (str == null || str == "Required*" || string.IsNullOrWhiteSpace(str))
            {
                return new ValidationResult(false, "Please enter a valid postcode");
            }
            // REGEX For postcodes provided by Gov.Uk --> UK Government
            if (!Regex.IsMatch(str, @"([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9][A-Za-z]?))))\s?[0-9][A-Za-z]{2})"))
                return new ValidationResult(false, String.Format("Please enter a valid postcode"));

            return new ValidationResult(true, null);

        }
    }
}
