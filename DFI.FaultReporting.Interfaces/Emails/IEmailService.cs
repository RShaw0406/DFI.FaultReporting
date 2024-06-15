using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Interfaces.Emails
{
    public interface IEmailService
    {
        Task<Response> SendEmail(string subject, EmailAddress to, string textContent, string htmlContent, Attachment? attachment);

        Task<Response> SendVerificationCodeEmail(EmailAddress to, int verficationToken);
    }
}
