using System;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BlazorBlogs.Data;
using BlazorBlogs.Data.Models;
using BlazorBlogsLibrary.Classes;
using BlazorBlogsLibrary.Data.Models;

namespace BlazorBlogsLibrary.Data
{
    public class EmailService
    {
        private readonly BlazorBlogsContext _context;
        private readonly GeneralSettingsService _generalSettingsService;

        public EmailService(BlazorBlogsContext context, GeneralSettingsService generalSettingsService)
        {
            _context = context;
            _generalSettingsService = generalSettingsService;
        }

        #region public async Task<string> SendMailAsync(bool SendAsync, string MailTo, string MailToDisplayName, string Cc, string Bcc, string ReplyTo, string Subject, string Body)

        public async Task<string> SendMailAsync(bool SendAsync, string MailTo, string MailToDisplayName, string Cc,
            string Bcc, string ReplyTo, string Subject, string Body)
        {
            var GeneralSettings = await _generalSettingsService.GetGeneralSettingsAsync();

            if (GeneralSettings.SMTPServer.Trim().Length == 0) return "Error: Cannot send email - SMTPServer not set";

            var arrAttachments = new string[0];

            return await SendMailAsync(SendAsync,
                GeneralSettings.SMTPFromEmail, MailTo, MailToDisplayName,
                Cc, Bcc, ReplyTo, MailPriority.Normal,
                Subject, Encoding.UTF8, Body, arrAttachments, "", "", "", "",
                GeneralSettings.SMTPSecure);
        }

        #endregion

        #region private async Task<string> SendMailAsync(bool SendAsync, string MailFrom, string MailTo, string MailToDisplayName, string Cc, string Bcc, string ReplyTo, System.Net.Mail.MailPriority Priority, string Subject, Encoding BodyEncoding, string Body, string[] Attachment, string SMTPServer, string SMTPAuthentication, string SMTPUsername, string SMTPPassword, bool SMTPEnableSSL)

        private async Task<string> SendMailAsync(bool SendAsync, string MailFrom, string MailTo,
            string MailToDisplayName, string Cc, string Bcc, string ReplyTo, MailPriority Priority,
            string Subject, Encoding BodyEncoding, string Body, string[] Attachment, string SMTPServer,
            string SMTPAuthentication, string SMTPUsername, string SMTPPassword, bool SMTPEnableSSL)
        {
            var strSendMail = "";
            var GeneralSettings = await _generalSettingsService.GetGeneralSettingsAsync();

            // SMTP server configuration
            if (SMTPServer == "")
            {
                SMTPServer = GeneralSettings.SMTPServer;

                if (SMTPServer.Trim().Length == 0) return "Error: Cannot send email - SMTPServer not set";
            }

            if (SMTPAuthentication == "") SMTPAuthentication = GeneralSettings.SMTPAuthendication;

            if (SMTPUsername == "") SMTPUsername = GeneralSettings.SMTPUserName;

            if (SMTPPassword == "") SMTPPassword = GeneralSettings.SMTPPassword;

            MailTo = MailTo.Replace(";", ",");
            Cc = Cc.Replace(";", ",");
            Bcc = Bcc.Replace(";", ",");

            MailMessage objMail = null;
            try
            {
                var SenderMailAddress = new MailAddress(MailFrom, MailFrom);
                var RecipientMailAddress = new MailAddress(MailTo, MailToDisplayName);

                objMail = new MailMessage(SenderMailAddress, RecipientMailAddress);

                if (Cc != "") objMail.CC.Add(Cc);
                if (Bcc != "") objMail.Bcc.Add(Bcc);

                if (ReplyTo != string.Empty) objMail.ReplyToList.Add(new MailAddress(ReplyTo));

                objMail.Priority = Priority;
                objMail.IsBodyHtml = IsHTMLMail(Body);

                foreach (var myAtt in Attachment)
                    if (myAtt != "")
                        objMail.Attachments.Add(new Attachment(myAtt));

                // message
                objMail.SubjectEncoding = BodyEncoding;
                objMail.Subject = Subject.Trim();
                objMail.BodyEncoding = BodyEncoding;

                var PlainView =
                    AlternateView.CreateAlternateViewFromString(Utility.ConvertToText(Body),
                        null, "text/plain");

                objMail.AlternateViews.Add(PlainView);

                //if body contains html, add html part
                if (IsHTMLMail(Body))
                {
                    var HTMLView =
                        AlternateView.CreateAlternateViewFromString(Body, null, "text/html");

                    objMail.AlternateViews.Add(HTMLView);
                }
            }
            catch (Exception objException)
            {
                // Problem creating Mail Object
                strSendMail = MailTo + ": " + objException.Message;

                // Log Error 
                var objLog = new Logs();
                objLog.LogDate = DateTime.Now;
                objLog.LogAction = $"{Constants.EmailError} - Error: {strSendMail}";
                objLog.LogUserName = MailTo;
                objLog.LogIpaddress = "127.0.0.1";
            }

            if (objMail != null)
            {
                // external SMTP server alternate port
                int? SmtpPort = null;
                var portPos = SMTPServer.IndexOf(":");
                if (portPos > -1)
                {
                    SmtpPort = int.Parse(SMTPServer.Substring(portPos + 1, SMTPServer.Length - portPos - 1));
                    SMTPServer = SMTPServer.Substring(0, portPos);
                }

                var smtpClient = new SmtpClient();

                if (SMTPServer != "")
                {
                    smtpClient.Host = SMTPServer;
                    smtpClient.Port = SmtpPort == null ? 25 : Convert.ToInt32(SmtpPort);

                    switch (SMTPAuthentication)
                    {
                        case "":
                        case "0":
                            // anonymous
                            break;
                        case "1":
                            // basic
                            if ((SMTPUsername != "") & (SMTPPassword != ""))
                            {
                                smtpClient.UseDefaultCredentials = false;
                                smtpClient.Credentials = new NetworkCredential(SMTPUsername, SMTPPassword);
                            }

                            break;
                        case "2":
                            // NTLM
                            smtpClient.UseDefaultCredentials = true;
                            break;
                    }
                }

                smtpClient.EnableSsl = SMTPEnableSSL;

                try
                {
                    if (SendAsync) // Send Email using SendAsync
                    {
                        // Set the method that is called back when the send operation ends.
                        smtpClient.SendCompleted += SmtpClient_SendCompleted;

                        // Send the email
                        var objMailMessage = new MailMessage();
                        objMailMessage = objMail;

                        smtpClient.SendAsync(objMail, objMailMessage);
                        strSendMail = "";
                    }
                    else // Send email and wait for response
                    {
                        smtpClient.Send(objMail);
                        strSendMail = "";

                        // Log the Email
                        LogEmail(objMail);

                        objMail.Dispose();
                        smtpClient.Dispose();
                    }
                }
                catch (Exception objException)
                {
                    // mail configuration problem
                    if (!(objException.InnerException == null))
                        strSendMail = string.Concat(objException.Message, Environment.NewLine,
                            objException.InnerException.Message);
                    else
                        strSendMail = objException.Message;

                    // Log Error 
                    var objLog = new Logs();
                    objLog.LogDate = DateTime.Now;
                    objLog.LogAction = $"{Constants.EmailError} - Error: {strSendMail}";
                    objLog.LogUserName = null;
                    objLog.LogIpaddress = "127.0.0.1";

                    _context.Logs.Add(objLog);
                    _context.SaveChanges();
                }
            }

            return strSendMail;
        }

        #endregion

        #region private void SmtpClient_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)

        private void SmtpClient_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            // Get the MailMessage object 
            var objMailMessage = (MailMessage) e.UserState;
            var objSmtpClient = (SmtpClient) sender;

            if (e.Error != null)
            {
                var objLog = new Logs();
                objLog.LogDate = DateTime.Now;
                objLog.LogAction =
                    $"{Constants.EmailError} - Error: {e.Error.GetBaseException().Message} - To: {objMailMessage.To} Subject: {objMailMessage.Subject}";
                objLog.LogUserName = null;
                objLog.LogIpaddress = "127.0.0.1";

                _context.Logs.Add(objLog);
                _context.SaveChanges();
            }
            else
            {
                // Log the Email
                LogEmail(objMailMessage);

                objMailMessage.Dispose();
                objSmtpClient.Dispose();
            }
        }

        #endregion

        #region IsHTMLMail

        public static bool IsHTMLMail(string Body)
        {
            return Regex.IsMatch(Body, "<[^>]*>");
        }

        #endregion

        #region private void LogEmail(System.Net.Mail.MailMessage objMailMessage)

        private void LogEmail(MailMessage objMailMessage)
        {
            // Loop through all recipients
            foreach (var item in objMailMessage.To)
            {
                var objLog = new Logs();
                objLog.LogDate = DateTime.Now;
                objLog.LogAction = $"{Constants.EmailSent} - To: {item.DisplayName} Subject: {objMailMessage.Subject}";
                objLog.LogUserName = item.Address;
                objLog.LogIpaddress = "127.0.0.1";

                _context.Logs.Add(objLog);
                _context.SaveChanges();
            }
        }

        #endregion
    }
}