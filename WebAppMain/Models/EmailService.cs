using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Threading.Tasks;

namespace WebAppMain.Models
{
    public class EmailService
    {
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("SCHOOL ADVANCEMENT", "rky36981@gmail.com"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.elasticemail.com", 2525, false);
                await client.AuthenticateAsync("rky36981@gmail.com", "A47CC2709F773B1FD1A68A176DDD1C232243");
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }
        }

        
    }
}
