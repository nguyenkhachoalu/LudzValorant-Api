using LudzValorant.Handle.HandleEmail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LudzValorant.InterfaceService
{
    public interface IEmailService
    {
        string SendEmail(EmailMessage emailMessage);
        string GenerateConfirmationCodeEmail(string confirmationCode);
        string GenerateProjectCompletionEmail(string projectName);
        string GenerateDeliveryCompletionEmail(string projectName);
        string GenerateForgotPassword(string newPassword);
    }
}
