using System;

namespace UpDiddyLib.Helpers
{
    public interface ISysEmail
    {
        bool SendEmail(string email, string subject, string htmlContent);
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