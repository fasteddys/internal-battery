using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UpDiddyLib.Helpers
{
    public interface ISysEmail
    {
        Task<bool> SendEmailAsync(string email, string subject, string htmlContent, Constants.SendGridAccount SendGridAccount);

        Task<bool> SendTemplatedEmailAsync(
            string email, 
            string templateId, 
            dynamic templateData,
            Constants.SendGridAccount SendGridAccount, 
            string subject = null, 
            List<Attachment> attachments = null,
            DateTime? sendAt = null,
            int? unsubscribeGroupId = null,
            string cc = null,
            string bcc = null);

        Task<bool> SendTemplatedEmailAsync(
            string[] to,
            string templateId,
            dynamic templateData,
            Constants.SendGridAccount SendGridAccount,
            string subject = null,
            List<Attachment> attachments = null,
            DateTime? sendAt = null,
            int? unsubscribeGroupId = null,
            string[] cc = null,
            string[] bcc = null);


        Task<bool> SendTemplatedEmailWithReplyToAsync(
            string email,
            string templateId,
            dynamic templateData,
            Constants.SendGridAccount SendGridAccount,
            string subject = null,
            List<Attachment> attachments = null,
            DateTime? sendAt = null,
            int? unsubscribeGroupId = null,
            string replyToEmail = null);



        void SendPurchaseReceiptEmail(
            string sendgridTemplateId,
            string profileUrl,
            string email,
            string subject,
            string courseName,
            decimal courseCost,
            decimal promoApplied,
            string formattedStartDate,
            Guid enrollmentGuid,
            string rebateToc);
    }
}