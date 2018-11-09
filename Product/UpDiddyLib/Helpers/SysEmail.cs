using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using UpDiddyLib.Dto;

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
        
        public async void SendPurchaseReceiptEmail(string email, string subject, string courseName, decimal courseCost, decimal promoApplied, Guid enrollmentGuid)
        {
            var client = new SendGridClient(_apiKey);
            var message = new SendGridMessage();
            message.SetFrom(new EmailAddress("support@careercircle.com", "CareerCircle Support"));
            message.AddTo(new EmailAddress(email));
            message.SetTemplateId(_configuration["SysEmail:TemplateIds:PurchaseReceipt"]);
            PurchaseReceipt purchaseReceipt = new PurchaseReceipt
            {
                Subject = subject,
                Profile_Url = _configuration["Environment:BaseUrl"],
                Course_Name = courseName,
                Course_Price = courseCost.ToString(),
                Discount = promoApplied.ToString(),
                Final_Price = (courseCost - promoApplied).ToString(),
                Enrollment_Guid = enrollmentGuid.ToString()
            };
            message.SetTemplateData(purchaseReceipt);
            var response = await client.SendEmailAsync(message);
                    
        }

        #region Private Helper Classes
        private class TemplateData
        {
            [JsonProperty("subject")]
            public string Subject { get; set; }
        }
        private class PurchaseReceipt : TemplateData
        {
            [JsonProperty("course_name")]
            public string Course_Name { get; set; }

            [JsonProperty("profile_url")]
            public string Profile_Url { get; set; }

            [JsonProperty("course_price")]
            public string Course_Price { get; set; }

            [JsonProperty("discount")]
            public string Discount { get; set; }

            [JsonProperty("final_price")]
            public string Final_Price { get; set; }

            [JsonProperty("enrollment_guid")]
            public string Enrollment_Guid { get; set; }
        }
        #endregion
    }
}
