using LudzValorant.Handle.HandleEmail;
using LudzValorant.InterfaceService;
using LudzValorant.Payloads.Responses;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LudzValorant.ImplementService
{
    public class EmailService : IEmailService
    {
        EmailConfiguration _emailConfiguration;

        public EmailService(EmailConfiguration emailConfiguration)
        {
            _emailConfiguration = emailConfiguration;
        }

        public string SendEmail(EmailMessage emailMessage)
        {
            var message = CreateEmailMessage(emailMessage);
            Send(message);
            var recipients = string.Join(", ", message.To);
            return ResponseMessage.GetEmailSuccessMessage(recipients);
        }

        private MimeMessage CreateEmailMessage(EmailMessage message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("email", _emailConfiguration.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;

            // Sử dụng BodyBuilder để tạo nội dung HTML cho email
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = message.Content
            };

            // Đặt nội dung HTML cho email
            emailMessage.Body = bodyBuilder.ToMessageBody();

            return emailMessage;
        }

        private void Send(MimeMessage message)
        {
            using var client = new SmtpClient();
            try
            {
                client.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.Port, true);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(_emailConfiguration.UserName, _emailConfiguration.Password);
                client.Send(message);
            }
            catch
            {
                throw;
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }
        public string GenerateConfirmationCodeEmail(string confirmationCode)
        {
            return $@"
            <html>
                <body style='font-family: Arial, sans-serif; text-align: center;'>
                    <div style='border: 1px solid #ddd; padding: 20px; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #333;'>Nhận mã xác nhận tài khoản</h2>
                        <p>Xin chào,</p>
                        <p>Bạn đã yêu cầu một mã xác nhận. Dưới đây là mã của bạn:</p>
                        <p style='font-size: 31px; font-weight: bold; color: #007BFF; background-color: #f0f0f0; padding: 10px; border-radius: 5px; letter-spacing: 10px; display: inline-block;'>{confirmationCode}</p>
                        <p>Vui lòng nhập mã này để xác nhận tài khoản của bạn.</p>
                        <br/>
                        <p>Chúc bạn một ngày tốt lành!</p>
                        <p>Trân trọng,</p>
                        <p><i>Đội ngũ hỗ trợ của chúng tôi</i></p>
                    </div>
                </body>
            </html>";
        }
        public string GenerateForgotPassword(string newPassword)
        {
            return $@"
            <html>
                <body style='font-family: Arial, sans-serif; text-align: center;'>
                    <div style='border: 1px solid #ddd; padding: 20px; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #333;'>Lấy lại mật khẩu</h2>
                        <p>Xin chào,</p>
                        <p>Bạn đã lấy lại tài khoản của mình thành công. Dưới đây là mật khẩu mới của bạn:</p>
                        <p style='font-size: 31px; font-weight: bold; color: #007BFF; background-color: #f0f0f0; padding: 10px; border-radius: 5px; letter-spacing: 10px; display: inline-block;'>{newPassword}</p>
                        <p>Vui lòng nhập mật khẩu này để đăng nhập tài khoản của bạn.</p>
                        <br/>
                        <p>Chúc bạn một ngày tốt lành!</p>
                        <p>Trân trọng,</p>
                        <p><i>Đội ngũ hỗ trợ của chúng tôi</i></p>
                    </div>
                </body>
            </html>";
        }
        public string GenerateProjectCompletionEmail(string projectName)
        {
            return $@"
            <html>
                <body style='font-family: Arial, sans-serif; text-align: center;'>
                    <div style='border: 1px solid #ddd; padding: 20px; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #333;'>Dự án {projectName} đã hoàn thành</h2>
                        <p>Xin chào,</p>
                        <p>Dự án của bạn đã được hoàn thành. Sẽ có thông báo cụ thể thời gian giao hàng tới quý khách</p>
                        <p>Cảm ơn bạn đã tin tưởng và sử dụng dịch vụ của chúng tôi.</p>
                        <br/>
                        <p>Chúc bạn một ngày tốt lành!</p>
                        <p>Trân trọng,</p>
                        <p><i>Đội ngũ hỗ trợ của chúng tôi</i></p>
                    </div>
                </body>
            </html>";
        }
        public string GenerateDeliveryCompletionEmail(string projectName)
        {
            return $@"
            <html>
                <body style='font-family: Arial, sans-serif; text-align: center;'>
                    <div style='border: 1px solid #ddd; padding: 20px; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #333;'>Dự án {projectName} đã được giao thành công</h2>
                        <p>Xin chào,</p>
                        <p>Dự án của bạn đã được giao hàng thành công.</p>
                        <p>Cảm ơn bạn đã tin tưởng và sử dụng dịch vụ của chúng tôi.</p>
                        <br/>
                        <p>Chúc bạn một ngày tốt lành!</p>
                        <p>Trân trọng,</p>
                        <p><i>Đội ngũ hỗ trợ của chúng tôi</i></p>
                    </div>
                </body>
            </html>";
        }
    }
}
