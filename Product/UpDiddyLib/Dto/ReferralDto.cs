namespace UpDiddyLib.Dto
{
    public class ReferralDto
    {
        // Code for referred job 
        public string JobReferralCode { get; set; }
        // Source of the subscriber as specified by the query string parameter of "source".  This was
        // introduced for viral recruiting pilot with EmployeeReferrals
        public string SubscriberSource { get; set; }
    }
}