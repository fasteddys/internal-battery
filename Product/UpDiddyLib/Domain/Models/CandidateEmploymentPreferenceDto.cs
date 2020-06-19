using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class CandidateEmploymentPreferenceDto
    {

        public bool? IsWillingToTravel { get; set; }

        public bool? IsFlexibleWorkScheduleRequired { get; set; }

        /// <summary>
        /// Guid that corresponds to CommuteDistance lookup.
        /// </summary>
        public Guid? CommuteDistanceGuid { get; set; }

        /// <summary>
        /// Guid that corresponds to a value in EmploymentType lookup.
        /// </summary>
        public List<Guid> EmploymentTypeGuids { get; set; }
    }
}
