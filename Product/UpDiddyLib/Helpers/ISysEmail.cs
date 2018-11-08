namespace UpDiddyLib.Helpers
{
    public interface ISysEmail
    {
        bool SendEmail(string email, string subject, string htmlContent);
        void SendPurchaseReceiptEmail(string email, string subject, string courseName, decimal courseCost, decimal promoApplied);
    }
}