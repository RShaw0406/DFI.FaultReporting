using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.SQL.Repository.Interfaces.Roles;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Emails
{
    public class EmailService : IEmailService
    {
        public ISettingsService _settingsService;

        public EmailService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<Response> SendEmail(string subject, EmailAddress to, string textContent, string htmlContent, Attachment? attachment)
        {
            var mailKey = await _settingsService.GetSettingString(DFI.FaultReporting.Common.Constants.Settings.EMAILKEY);
            var mailClient = new SendGridClient(mailKey);
            var from = new EmailAddress(await _settingsService.GetSettingString(DFI.FaultReporting.Common.Constants.Settings.EMAILFROM));
            //var subject = "Sending with SendGrid is Fun";
            //var to = new EmailAddress(loginRequest.Email);
            //var plainTextContent = "and easy to do anywhere, even with C#";
            //var htmlContent = "<strong>and easy to do anywhere, even with C#</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, textContent, htmlContent);
            //var attachment = new Attachment();
            if (attachment != null)
            {
                msg.Attachments.Add(attachment);
            }

            return await mailClient.SendEmailAsync(msg);
        }
    }
}
