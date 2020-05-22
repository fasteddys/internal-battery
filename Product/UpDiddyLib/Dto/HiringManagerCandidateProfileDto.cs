using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class HiringManagerCandidateProfileDto
    {
        /// <summary>
        /// Candidate's profile guid.
        /// </summary>
        public Guid ProfileGuid { get; set; }

        /// <summary>
        /// Candidate's last employment title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Candidate's last emploment location - city.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Candidate's last emploment location - state.
        /// </summary>
        public string State { get; set; }

    }
}
