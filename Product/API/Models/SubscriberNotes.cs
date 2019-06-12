using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class SubscriberNotes :BaseModel
    {
        public int SubscriberNotesId { get; set; }
        public Guid SubscriberNotesGuid { get; set; }
        public int SubscriberId { get; set; }
        public int RecruiterId { get; set; }
        public string Notes { get; set; }
        public bool ViewableByOthersInRecruiterCompany { get; set; } 
    }
}
