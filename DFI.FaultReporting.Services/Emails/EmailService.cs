using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.SQL.Repository.Interfaces.Roles;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
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

        public async Task<Response> SendEmail(string subject, EmailAddress to, string textContent, string htmlContent, SendGrid.Helpers.Mail.Attachment? attachment)
        {
            var mailKey = await _settingsService.GetSettingString(DFI.FaultReporting.Common.Constants.Settings.EMAILKEY);
            var mailClient = new SendGridClient(mailKey);
            var from = new EmailAddress(await _settingsService.GetSettingString(DFI.FaultReporting.Common.Constants.Settings.EMAILFROM));
            var msg = MailHelper.CreateSingleEmail(from, to, subject, textContent, htmlContent);
            if (attachment != null)
            {
                msg.Attachments.Add(attachment);
            }

            return await mailClient.SendEmailAsync(msg);
        }

        public async Task<Response> SendVerificationCodeEmail(EmailAddress to, int verficationToken)
        {
            var mailKey = await _settingsService.GetSettingString(DFI.FaultReporting.Common.Constants.Settings.EMAILKEY);
            var mailClient = new SendGridClient(mailKey);
            var from = new EmailAddress(await _settingsService.GetSettingString(DFI.FaultReporting.Common.Constants.Settings.EMAILFROM));
            string subject = "DFI Fault Reporting: Verification Code";
            string textContent = string.Empty;
            string htmlContent = "<p>Hello,</p><p>Below is your verification code, do not share this code with anyone.</p><br /><p>Verification Code:</p><br /><strong>" + verficationToken.ToString() + "</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, textContent, htmlContent);

            return await mailClient.SendEmailAsync(msg);
        }
    }
}
