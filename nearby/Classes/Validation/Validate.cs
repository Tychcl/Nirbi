using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel.Communication;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace nearby.Classes.Validation
{
    public static class Validate
    {
        public static Func<string, bool> EmailValidator => EmailAdress;
        public static Func<string, bool> FIOValidator => FIO;
        public static Func<string, bool> PhoneValidator => PhoneNumber;
        public static Func<string, bool> EmailOrPhoneValidator => EmailOrPhone;
        public static Func<string, bool> PasswordValidator => Password;
        public static Func<string, bool> NonEmptyValidator => str => !string.IsNullOrWhiteSpace(str);

        public static bool EmailAdress(string str)
        {
            return string.IsNullOrWhiteSpace(str) ? false : Regex.IsMatch(str.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
        }

        public static bool PhoneNumber(string str)
        {
            return string.IsNullOrWhiteSpace(str) ? false : Regex.IsMatch(str.Trim(), @"^\+[0-9]{10,15}$");
        } 

        public static bool EmailOrPhone(string str)
        {
            return string.IsNullOrWhiteSpace(str) ? false : EmailAdress(str) || PhoneNumber(str);
        }

        public static bool Password(string str)
        {
            return string.IsNullOrWhiteSpace(str) ? false : Regex.IsMatch(str.Trim(), "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-._]).{8,}$");
        }

        public static bool FIO(string str)
        {
            return string.IsNullOrWhiteSpace(str) ? false : Regex.IsMatch(str.Trim(), @"^[А-ЯЁ][а-яё]+\s[А-ЯЁ][а-яё]+\s[А-ЯЁ][а-яё]+$");
        }
    }
}
