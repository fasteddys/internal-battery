
using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class TraitifyDto
    {
         public Guid TraitifyGuid { get; set; }
        public Guid? SubscriberGuid { get; set; }
        public string AssessmentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string DeckId { get; set; }
        public string ResultData { get; set; }
        public string PublicKey {get;set;}
        public string Host {get;set;}
    }
}