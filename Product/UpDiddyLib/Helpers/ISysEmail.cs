namespace UpDiddyLib.Helpers
{
    public interface ISysEmail
    {
        bool SendEmail(string email, string subject, string htmlContent);
    }
}