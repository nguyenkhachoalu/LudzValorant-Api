using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LudzValorant.Payloads.Responses
{
    public static class ResponseMessage
    {
        public static string GetEmailSuccessMessage(string email)
        {
            return $"Email đã được gửi đến: {email}";
        }
    }
}
