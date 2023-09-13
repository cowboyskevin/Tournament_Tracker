using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using TrackerLibrary;

namespace Tournament_Tracker
{
    public static class EmailLogic
    {
        public static void SendEmail(List<string> to, List<string> bcc, string subject, string body)
        {
                MailAddress fromMailAddress = new MailAddress(GlobalConfig.AppKeyLookup("senderEmail"), GlobalConfig.AppKeyLookup("senderDisplayName"));

                MailMessage mailMessage = new MailMessage();

                foreach (string email in to)
                {
                    mailMessage.To.Add(email); 
                }
                foreach (string email in bcc)
                {
                    mailMessage.Bcc.Add(email);
                }
                mailMessage.From = fromMailAddress;
                mailMessage.Subject = subject;
                mailMessage.Body = body;

                mailMessage.IsBodyHtml = true;

                SmtpClient client = new SmtpClient();

                client.Send(mailMessage); 
        }
        public static void SendEmail(string to, string subject, string body)
        {
            SendEmail(new List<string> { to }, new List<string>(), subject, body);
        }
    }
}
