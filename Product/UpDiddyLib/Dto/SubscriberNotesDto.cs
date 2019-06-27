using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
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
    }
}
