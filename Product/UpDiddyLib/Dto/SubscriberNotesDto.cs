using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UpDiddyLib.Dto
{
    public class SubscriberNotesListDto
    {
        public List<SubscriberNotesDto> SubscriberNotes { get; set; } = new List<SubscriberNotesDto>();
        public int TotalRecords { get; set; }
    }

    public class SubscriberNotesDto
    {
        public Guid? SubscriberNotesGuid { get; set; }
        public Guid SubscriberGuid { get; set; }
        public Guid RecruiterGuid { get; set; }
        public string RecruiterName { get; set; }
        public string Notes { get; set; }
        public bool ViewableByOthersInRecruiterCompany { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}
