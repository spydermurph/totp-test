using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace MoviesAPI.Services
{
    public interface IEmailService
    {
        void Send(string from, string to, string subject, string message);
    }
    public class EmailService : IEmailService
    {
        public void Send(string from, string to, string subject, string message)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(from));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Text);

            using var smtp = new SmtpClient();
            smtp.Connect("smtp-mail.outlook.com", 587, SecureSocketOptions.StartTls);
            smtp.Authenticate("", "");
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
