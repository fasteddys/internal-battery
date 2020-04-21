using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class UserDefinedEmailDto
    {
        /// <summary>
        /// These profiles are the email recipients. Required.
        /// </summary>
        public List<Guid> Profiles { get; set; }

        /// <summary>
        /// This is the email's body/content. Required.
        /// </summary>
        public String EmailTemplate { get; set; }

        /// <summary>
        /// Subject of the email. Required.
        /// </summary>
        public String Subject { get; set; }

        /// <summary>
        /// The reply to recipient. Required.
        /// </summary>
        public String ReplyToEmailAddress { get; set; }

        /// <summary>
        /// An activity note from the subscriber. Optional.
        /// </summary>
        public String ActivityNote { get; set; }
    }
}
