using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;


namespace UpDiddyLib.Helpers
{
    public class SysEmail : ISysEmail
    {

        private IConfiguration _configuration;
        private string _apiKey = string.Empty;
        public SysEmail(IConfiguration Configuration)
        {
            _configuration = Configuration;
            _apiKey = Configuration["SysEmail:ApiKey"];
        }
  
 
        public bool SendEmail(string email, string subject, string htmlContent)
        {
            // TODO put key in vault 
           // var apiKey = "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";
            var client = new SendGridClient(_apiKey);
            SendGrid.Helpers.Mail.EmailAddress from = new EmailAddress("support@careercircle.com", "CareerCircle Support");
            SendGrid.Helpers.Mail.EmailAddress to = new EmailAddress(email);
            var plainTextContent = Regex.Replace(htmlContent, "<[^>]*>", "");
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = AsyncHelper.RunSync<Response>(() => client.SendEmailAsync(msg));
            return true;
        }
         
    }
}
