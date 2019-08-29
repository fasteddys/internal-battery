using System;
using UpDiddyLib.Dto;

namespace UpDiddyApi.Models
{
    public class Traitify : BaseDto
    {
        public int Id { get; set; }
        public Guid TraitifyGuid { get; set; }
        public Subscriber Subscriber { get; set; }
        public Guid? SubscriberGuid { get; set; }
        public int? SubscriberId { get; set; }
        public string AssessmentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string DeckId { get; set; }
        public string ResultData { get; set; }
    }
}
