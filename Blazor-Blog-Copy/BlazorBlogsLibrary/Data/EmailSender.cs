using System;
using System.Threading.Tasks;
using BlazorBlogs.Data;
using BlazorBlogs.Data.Models;
using BlazorBlogsLibrary.Data.Models;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BlazorBlogsLibrary.Data
{
    public class EmailSender : IEmailSender
    {
        private readonly BlazorBlogsContext _context;
        private readonly EmailService _EmailService;
        private readonly GeneralSettingsService _GeneralSettingsService;

        public EmailSender(BlazorBlogsContext context, EmailService EmailService,
            GeneralSettingsService generalSettingsService)
        {
            _context = context;
            _EmailService = EmailService;
            _GeneralSettingsService = generalSettingsService;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            return EmailSendAsync(email, subject, message);
        }

        public async Task EmailSendAsync(string email, string subject, string message)
        {
            var objGeneralSettings = await _GeneralSettingsService.GetGeneralSettingsAsync();

            var strError = await _EmailService.SendMailAsync(
                false,
                email,
                email,
                "", "",
                objGeneralSettings.SMTPFromEmail,
                $"Account Confirmation From: {objGeneralSettings.ApplicationName} {subject}",
                $"This is an account confirmation email from: {objGeneralSettings.ApplicationName}. {message}");

            if (strError != "")
            {
                var objLog = new Logs();
                objLog.LogDate = DateTime.Now;
                objLog.LogUserName = email;
                objLog.LogIpaddress = "127.0.0.1";
                objLog.LogAction =
                    $"{Constants.EmailError} - Error: {strError} - To: {email} Subject: Account Confirmation From: {objGeneralSettings.ApplicationName} {subject}";
                _context.Logs.Add(objLog);
                _context.SaveChanges();
            }
        }
    }
}