using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LudzValorant.Validations
{
    public class ValidateInput
    {
        public static bool IsValidEmail(string email)
        {
            var emailAttribute = new EmailAddressAttribute();
            return emailAttribute.IsValid(email);
        }
        public static bool isValidPhoneNumber(string phoneNumber)
        {
            // Biểu thức chính quy để xác thực số điện thoại
            string pattern = @"^(\+?[0-9]{1,3})?([ .-]?[0-9]{3}){2}[ .-]?[0-9]{4}$";
            return Regex.IsMatch(phoneNumber, pattern);
        }
    }
}
