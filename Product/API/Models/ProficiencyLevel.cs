using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UpDiddyApi.Models
{
    public class ProficiencyLevel : BaseModel
    {
        public int ProficiencyLevelId { get; set; }

        public Guid ProficiencyLevelGuid { get; set; }

        [Required]
        [StringLength(500)]
        public string ProficiencyLevelName { get; set; }

        public int Sequence { get; set; }

        public virtual List<SubscriberLanguageProficiency> SubscriberLanguageProficiencies { get; set; }
    }
}
