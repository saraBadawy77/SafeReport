using System.Net;
using System.Net.Mail;

namespace SafeReport.Application.Helper
{
    public static class EmailSender
    {
        private static readonly string SenderEmail = "sarah77nashat77@gmail.com";
        private static readonly string SenderPassword = "qmgldngpjircsppg";

        public static async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(SenderEmail, SenderPassword)
            };

            var message = new MailMessage(SenderEmail, toEmail, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);
        }
    }
}
