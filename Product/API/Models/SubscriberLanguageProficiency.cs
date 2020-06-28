using System;

namespace UpDiddyApi.Models
{
    public class SubscriberLanguageProficiency : BaseModel
    {
        public int SubscriberLanguageProficiencyId { get; set; }

        public Guid SubscriberLanguageProficienciesGuid { get; set; }

        public int SubscriberId { get; set; }

        public virtual Subscriber Subscriber { get; set; }

        public int LanguageId { get; set; }

        public virtual Language Language { get; set; }

        public int ProficiencyLevelId { get; set; }

        public virtual ProficiencyLevel ProficiencyLevel { get; set; }
    }
}
