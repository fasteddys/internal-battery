using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace UpDiddyLib.Helpers
{
    public class SysEmail
    {

        // TODO build out with 
        // SystemEmail
        // SystemErrorEmail
           // TODO get setting from vault 
 
 
        static private bool SendEmail(string email, string subject, string htmlContent)
        {
            // TODO put key in vault 
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            SendGrid.Helpers.Mail.EmailAddress from = new EmailAddress("support@careercircle.com", "CareerCircle Support");
            SendGrid.Helpers.Mail.EmailAddress to = new EmailAddress(email);
            var plainTextContent = Regex.Replace(htmlContent, "<[^>]*>", "");
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = AsyncHelper.RunSync<Response>(() => client.SendEmailAsync(msg));

            return true;
        }
        
    }
}
