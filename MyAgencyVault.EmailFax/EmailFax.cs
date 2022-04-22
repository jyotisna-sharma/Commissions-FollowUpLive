using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Threading;

namespace MyAgencyVault.EmailFax
{
    public class EmailFax
    {
        public string From = string.Empty;
        public string To = string.Empty;
        public string User = string.Empty;
        public string Password = string.Empty;
        public string Subject = string.Empty;
        public string Body = string.Empty;
        public string AttachmentPath = string.Empty;
        public string Host = "127.0.0.1";
        public int Port = 587;
        public string CC = string.Empty;
        public bool IsHtml = false;
        public int SendUsing = 0;//0 = Network, 1 = PickupDirectory, 2 = SpecifiedPickupDirectory
        public bool UseSSL = true;
        public int AuthenticationMode = 1;//0 = No authentication, 1 = Plain Text, 2 = NTLM authentication

        public EmailFax(string UserName,string Passowrd,string FromEmail,string ToEmail,string HostName,string PortNo,string MailSubject,string MailBody )
        {
            User = UserName;
            Password = Passowrd;
            From = FromEmail;
            To = ToEmail;            
            Host = HostName;
            Port = Convert.ToInt32(PortNo);
            Subject = MailSubject;
            Body = MailBody;
        }
        public void SendEmail()
        {
            new Thread(new ThreadStart(SendMessage)).Start();
        }
        /// <summary>
        /// Send Email Message method.
        /// </summary>
        private void SendMessage()
        {
            try
            {
                MailMessage oMessage = new MailMessage();
                SmtpClient smtpClient = new SmtpClient(Host);

                oMessage.From = new MailAddress(From);
                oMessage.To.Add(To);
                oMessage.Subject = Subject;
                oMessage.IsBodyHtml = IsHtml;
                oMessage.Body = Body;

                if (CC != string.Empty)
                    oMessage.CC.Add(CC);

                switch (SendUsing)
                {
                    case 0:
                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                        break;
                    case 1:
                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis;
                        break;
                    case 2:
                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                        break;
                    default:
                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                        break;

                }
                if (AuthenticationMode > 0)
                {
                    smtpClient.Credentials = new NetworkCredential(User, Password);
                }

                smtpClient.Port = Port;
                smtpClient.EnableSsl = UseSSL;
                // Create and add the attachment
                if (AttachmentPath != string.Empty)
                {
                    oMessage.Attachments.Add(new Attachment(AttachmentPath));
                }

                try
                {
                    // Deliver the message    
                    smtpClient.Send(oMessage);
                }

                catch (Exception ex)
                {
                    ex.ToString();
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }
    }
}
