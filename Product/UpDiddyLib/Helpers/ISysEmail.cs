using System;
using System.Threading.Tasks;

namespace UpDiddyLib.Helpers
{
    public interface ISysEmail
    {
        Task<bool> SendEmailAsync(string email, string subject, string htmlContent);

        Task<bool> SendTemplatedEmailAsync(string email, string templateId, dynamic templateData, string subject = null, string attachment = null, string attachmentName = null);

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