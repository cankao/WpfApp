using System.Linq;
using System.Text.RegularExpressions;

namespace WpfApp.Services
{
    public static class CpfValidator
    {
        public static string OnlyDigits(string cpf)
        {
            if (string.IsNullOrEmpty(cpf)) return string.Empty;
            return Regex.Replace(cpf, "[^0-9]", "");
        }

        public static string Format(string cpf)
        {
            var digits = OnlyDigits(cpf);
            if (digits.Length != 11) return cpf;
            return digits.Substring(0, 3) + "." + digits.Substring(3, 3) + "." +
                   digits.Substring(6, 3) + "-" + digits.Substring(9, 2);
        }

        public static bool IsValid(string cpf)
        {
            var digits = OnlyDigits(cpf);
            if (digits.Length != 11) return false;
            if (digits.Distinct().Count() == 1) return false;

            int sum = 0;
            for (int i = 0; i < 9; i++) sum += (digits[i] - '0') * (10 - i);
            int rest = sum % 11;
            int d1 = rest < 2 ? 0 : 11 - rest;
            if (d1 != (digits[9] - '0')) return false;

            sum = 0;
            for (int i = 0; i < 10; i++) sum += (digits[i] - '0') * (11 - i);
            rest = sum % 11;
            int d2 = rest < 2 ? 0 : 11 - rest;
            return d2 == (digits[10] - '0');
        }
    }
}
