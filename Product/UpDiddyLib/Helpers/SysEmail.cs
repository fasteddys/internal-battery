using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using UpDiddyLib.Dto;
using System.Threading.Tasks;

namespace UpDiddyLib.Helpers
{
    public class SysEmail : ISysEmail
    {
        private IConfiguration _configuration;
        public SysEmail(IConfiguration Configuration)
        {
            _configuration = Configuration;
        }

        public async Task<bool> SendEmailAsync(string email, string subject, string htmlContent, Constants.SendGridAccount SendGridAccount)
        {
            bool isDebugMode = _configuration[$"SysEmail:DebugMode"] == "true";
            string SendGridAccountType = Enum.GetName(typeof(Constants.SendGridAccount), SendGridAccount);
            var client = new SendGridClient(_configuration[$"SysEmail:{SendGridAccountType}:ApiKey"]);

            SendGrid.Helpers.Mail.EmailAddress from = new EmailAddress(_configuration[$"SysEmail:{SendGridAccountType}:FromEmailAddress"], "CareerCircle Support");
            SendGrid.Helpers.Mail.EmailAddress to = null;
            // check debug mode to only send emails to actual users in the system is not in debug mode 
            if ( isDebugMode == false )
                to = new EmailAddress(email);
            else 
                to = new EmailAddress(_configuration[$"SysEmail:SystemDebugEmailAddress"]);

            var plainTextContent = Regex.Replace(htmlContent, "<[^>]*>", "");
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            // Add custom subject property that will be sent to the webhook            
            msg.CustomArgs = new Dictionary<string, string>()
            {
                { "Subject", "this is the subject" }
            };
            var response = await client.SendEmailAsync(msg);
            return true;
        }

        public async Task<bool> SendTemplatedEmailAsync(string email, string templateId, dynamic templateData, Constants.SendGridAccount SendGridAccount, string subject = null, List<Attachment> attachments = null, DateTime? sendAt = null, int? unsubscribeGroupId = null)
        {
            bool isDebugMode = _configuration[$"SysEmail:DebugMode"] == "true";
            string SendGridAccountType = Enum.GetName(typeof(Constants.SendGridAccount), SendGridAccount);

            var client = new SendGridClient(_configuration[$"SysEmail:{SendGridAccountType}:ApiKey"]);
            var message = new SendGridMessage();
            if (sendAt.HasValue)
                message.SendAt = Utils.ToUnixTimeInSeconds(sendAt.Value);
            message.SetFrom(new EmailAddress(_configuration[$"SysEmail:{SendGridAccountType}:FromEmailAddress"], "CareerCircle"));
            message.SetReplyTo(new EmailAddress(_configuration[$"SysEmail:{SendGridAccountType}:ReplyToEmailAddress"]));

            // check debug mode to only send emails to actual users in the system is not in debug mode 
            if (isDebugMode == false)
                message.AddTo(new EmailAddress(email));
            else
                message.AddTo(new EmailAddress(_configuration[$"SysEmail:SystemDebugEmailAddress"]));

            message.SetTemplateId(templateId);
            message.SetTemplateData(templateData);

            // include the unsubscribe group for the sub-account if one is specified. 
            // note that it is not possible to associate unsubscribe groups from the parent account or other sub-accounts. 
            // (attempting to do this causes the email to be dropped by SendGrid with the following error message: This email was not sent because the SMTPAPI header was invalid.)
            if (unsubscribeGroupId.HasValue)
                message.SetAsm(unsubscribeGroupId.Value, new List<int>() { unsubscribeGroupId.Value });
            
            if(attachments != null)
                message.AddAttachments(attachments);
            if (subject != null)
                message.SetSubject(subject);

            // Add custom property that will be sent to the webhook. For templated emails, we will use the templated ID as the subject since the actual
            // subject of the template is not readily available 
            string webhookSubject = $"CC Template: {templateData}";
            if (subject != null)
                webhookSubject += $" with a subject of {subject}";

            message.CustomArgs = new Dictionary<string, string>()
            {
                { "Subject", webhookSubject }
            };

            var response = await client.SendEmailAsync(message);
            int statusCode = (int)response.StatusCode;

            return statusCode >= 200 && statusCode <= 299;
        }
        
        public async void SendPurchaseReceiptEmail(
            string sendgridTemplateId,
            string profileUrl,
            string email, 
            string subject, 
            string courseName, 
            decimal courseCost, 
            decimal promoApplied, 
            string formattedStartDate,
            Guid enrollmentGuid,
            string rebateToc)
        {
            var client = new SendGridClient(_configuration["SysEmail:Transactional:ApiKey"]);
            var message = new SendGridMessage();
            message.SetFrom(new EmailAddress(_configuration["SysEmail:Transactional:FromEmailAddress"], "CareerCircle"));
            message.SetReplyTo(new EmailAddress(_configuration["SysEmail:Transactional:ReplyToEmailAddress"]));
            message.AddTo(new EmailAddress(email));
            message.SetTemplateId(sendgridTemplateId);
            PurchaseReceipt purchaseReceipt = new PurchaseReceipt
            {
                Subject = subject,
                Profile_Url = profileUrl,
                Course_Name = courseName,
                Course_Price = courseCost.ToString(),
                Discount = promoApplied.ToString(),
                Final_Price = (courseCost - promoApplied).ToString(),
                Start_Date = formattedStartDate,
                Enrollment_Guid = enrollmentGuid.ToString(),
                Rebate_Toc = rebateToc
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

            [JsonProperty("start_date")]
            public string Start_Date { get; set; }

            [JsonProperty("enrollment_guid")]
            public string Enrollment_Guid { get; set; }

            [JsonProperty("rebate_toc")]
            public string Rebate_Toc { get; set; }
        }
        #endregion
    }
}
