using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class ProfileViewDto
    {

        public Guid? SubscriberGuid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string StateCode { get; set; }
        public string PostalCode { get; set; }
        // probably should not expose this in a view model but not worried about fixing that with the site rebuild coming
        public string CloudTalentUri { get; set; }
        public Guid? CloudIdentifier
        {
            get
            {
                Guid _cloudIdentifier;
                var idx = CloudTalentUri?.LastIndexOf('/');
                if (idx.HasValue)
                {
                    if (Guid.TryParse(CloudTalentUri.Substring(idx.Value + 1), out _cloudIdentifier))
                        return _cloudIdentifier;
                }
                return null;
            }
        }
        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }


        /// <summary>
        /// name of partner who is responsible for subscriber joining carreer circle
        /// </summary>
        public List<string> SourcePartner { get; set; }
        public List<SkillDto> Skills { get; set; }
        public List<SubscriberWorkHistoryDto> WorkHistory { get; set; }
        public List<SubscriberEducationHistoryDto> EducationHistory { get; set; }
    }
}
