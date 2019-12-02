

using System.ComponentModel.DataAnnotations;
namespace UpDiddyLib.Domain.Models
{
    public class TraitifyRequestDto
    {
        // public Guid TraitifyGuid { get; set; }
        // public Guid? SubscriberGuid { get; set; }
        // public int? SubscriberId { get; set; }
        // public string AssessmentId { get; set; }
        public string AssessmentId {get;set;}
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$", ErrorMessage = "Invalid email address. Please update your supplied email and try again.")]
        public string Email { get; set; }
        // public string Password { get; set; }
        // public DateTime? CompleteDate { get; set; }
        // public string DeckId { get; set; }
        // public string ResultData { get; set; }
        // public int ResultLength { get; set; }
        // public string PublicKey { get; set; }
        // public string Host { get; set; }
        // public bool IsComplete { get; set; }
        // public bool IsRegistered { get; set; }

    }
}