using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Mail.Net
{
    public class Mailing
    {
        NetworkCredential _credentials;
        string _smtpHost;
        int _smtpPort;
        bool _enableSsl;
        MailAddress _fromAddress;

        public Mailing(string username, string password, string smtpHost, int smtpPort, string fromAddress, bool enableSsl)
        {
            _credentials = new NetworkCredential(username, password);
            _smtpHost = smtpHost;
            _smtpPort = smtpPort;
            _enableSsl = enableSsl;
            _fromAddress = new MailAddress(fromAddress);
        }

        public void SendMail(string recipients, string bcc, string subject, string body, Action sendCompletedAction, Action<Exception> onExceptionAction, List<Attachment> attachments = null)
        {
            var client = _createSmtpClient();
            var message = _createMailMessage(recipients, bcc, subject, body, attachments);

            try
            {
                client.Send(message);
                sendCompletedAction.Invoke();
            }
            catch (Exception ex)
            {
                onExceptionAction.Invoke(ex);
            }
            finally
            {
                client.Dispose();
                message.Dispose();
            }
        }

        public void SendMailAsync(string recipients, string bcc, string subject, string body, Action sendCompletedAction, Action<Exception> onExceptionAction, List<Attachment> attachments = null)
        {
            var client = _createSmtpClient();
            var message = _createMailMessage(recipients, bcc, subject, body, attachments);
            client.SendCompleted += (s, e) =>
            {
                sendCompletedAction.Invoke();
                client.Dispose();
                message.Dispose();
            };

            try
            {
                client.SendAsync(message, null);
            }
            catch (Exception ex)
            {
                onExceptionAction.Invoke(ex);
            }
        }

        private MailMessage _createMailMessage(string recipients, string bcc, string subject, string body, List<Attachment> attachments = null)
        {
            MailMessage msg = new MailMessage();

            msg.From = _fromAddress;
            msg.Subject = subject;
            msg.Body = body;
            msg.IsBodyHtml = body.Contains("/>");

            foreach (string r in _splitRecipients(recipients))
                msg.To.Add(r);
            foreach (string b in _splitRecipients(bcc))
                msg.Bcc.Add(b);

            if (attachments != null)
                foreach (Attachment att in attachments)
                    msg.Attachments.Add(att);
            return msg;
        }

        private SmtpClient _createSmtpClient()
        {
            SmtpClient smtp = new SmtpClient();
            smtp.Host = _smtpHost;
            smtp.Port = _smtpPort;
            smtp.Timeout = 20000;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Credentials = _credentials;
            smtp.EnableSsl = _enableSsl;
            return smtp;
        }

        private string[] _splitRecipients(string recipientsList)
        {
            if (!string.IsNullOrEmpty(recipientsList))
                return recipientsList.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            else
                return new string[0];
        }
    }
}
