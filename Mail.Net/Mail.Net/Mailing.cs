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

        /// <summary>
        /// Initialises an instance of Mailing.
        /// </summary>
        /// <param name="username">Username for the SMTP Server</param>
        /// <param name="password">Password for the SMTP Server</param>
        /// <param name="smtpHost">Host address for the SMTP Server</param>
        /// <param name="smtpPort">Port for the SMTP Server</param>
        /// <param name="fromAddress">Address that mail will be sent from</param>
        /// <param name="enableSsl">Boolean to indicate whether we will use SSL</param>
        /// <param name="mailFrom">The text to appear in the 'From' field in the sent mail</param>
        public Mailing(string username, string password, string smtpHost, int smtpPort, string fromAddress, bool enableSsl)
        {
            _credentials = new NetworkCredential(username, password);
            _smtpHost = smtpHost;
            _smtpPort = smtpPort;
            _enableSsl = enableSsl;
            _fromAddress = new MailAddress(fromAddress);
        }

        /// <summary>
        /// Standard, synchronous mail sending method
        /// </summary>
        /// <param name="recipients">Recipients, can be semi-colon delimited</param>
        /// <param name="bcc">BCC recipients, can be semi-colon delimited</param>
        /// <param name="subject">Subject text</param>
        /// <param name="body">Body text. Can be HTML or plaintext</param>
        /// <param name="sendCompletedAction">An action to be performed when send is complete</param>
        /// <param name="onExceptionAction">An action to be performed on exception</param>
        /// <param name="attachments">Attachments we want to add to the mail</param>
        public void SendMail(string recipients, string bcc, string subject, string body, Action sendCompletedAction = null, Action<Exception> onExceptionAction = null, List<Attachment> attachments = null)
        {
            var client = _createSmtpClient();
            var message = _createMailMessage(recipients, bcc, subject, body, attachments);

            try
            {
                client.Send(message);
                sendCompletedAction?.Invoke();
            }
            catch (Exception ex)
            {
                onExceptionAction?.Invoke(ex);
            }
            finally
            {
                client.Dispose();
                message.Dispose();
            }
        }

        /// <summary>
        /// Asynchronous mail sending method
        /// </summary>
        /// <param name="recipients">Recipients, can be semi-colon delimited</param>
        /// <param name="bcc">BCC recipients, can be semi-colon delimited</param>
        /// <param name="subject">Subject text</param>
        /// <param name="body">Body text. Can be HTML or plaintext</param>
        /// <param name="sendCompletedAction">An action to be performed when send is complete</param>
        /// <param name="onExceptionAction">An action to be performed on exception</param>
        /// <param name="attachments">Attachments we want to add to the mail</param>
        public async Task SendMailAsync(string recipients, string bcc, string subject, string body, Action sendCompletedAction = null, Action<Exception> onExceptionAction = null, List<Attachment> attachments = null)
        {
            var client = _createSmtpClient();
            var message = _createMailMessage(recipients, bcc, subject, body, attachments);

            client.SendCompleted += (s, e) =>
            {
                sendCompletedAction?.Invoke();
                client.Dispose();
                message.Dispose();
            };

            try
            {
                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                onExceptionAction?.Invoke(ex);
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
