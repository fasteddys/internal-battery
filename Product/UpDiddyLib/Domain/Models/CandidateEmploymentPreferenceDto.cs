using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class CandidateEmploymentPreferenceDto
    {
        /// <summary>
        /// Subscriber guid of the candidate.
        /// Used when the Dto returns a hydrated object.
        /// </summary>
        public Guid? CandidatesSubscriberGuid { get; set; }

        public bool? IsWillingToTravel { get; set; }

        public bool? IsFlexibleWorkScheduleRequired { get; set; }

        /// <summary>
        /// Guid that corresponds to CommuteDistance lookup.
        /// </summary>
        public Guid? CommuteDistanceGuid { get; set; }

        /// <summary>
        /// Guid that corresponds to a value in EmploymentType lookup.
        /// </summary>
        public List<Guid> PreferredPlacementTypes { get; set; }
    }
}
