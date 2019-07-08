using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class GoogleCloudProfile
    {
        public string name { get; set; }
        public string externalId { get; set; }
        public Timestamp createTime { get; set; }
        public Timestamp updateTime { get; set; }

        public IList<PersonName> personNames { get; set; }
        public IList<Address> addresses { get; set; }

        public IList<Skill> skills { get; set; }

        public IList<EmailAddress> emailAddresses { get; set; }

        public IList<PhoneNumber> phoneNumbers { get; set; }


        public IList<EmploymentRecord> employmentRecords { get; set; }

        public IList<EducationRecord> educationRecords { get; set; }

        public Dictionary<string, CustomAttribute> customAttributes { get; set; }

        public string uri { get; set; }

        public string groupId { get; set; }

        public BoolValue isHirable { get; set; }

        public bool processed { get; set; }

        public string keywordSnippet { get; set; }

        public Resume resume { get; set; }

        public List<PersonalUri> personalUris { get; set; }

        public List<AdditionalContactInfo> additionalContactInfo {get; set;}

        public List<Activity> activities { get; set; }

        public List<Publication> publications { get; set; }

        public List<Patent> patents { get; set; }

        public List<Certification> certifications { get; set; } 

        public List<string> applications { get; set; }

        public List<string> assignments { get; set; }


    }
}
