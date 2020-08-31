using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpDiddyApi.Models
{
    [Table("ProhibitiedEmailDomains", Schema = "B2B")]
    public class ProhibitiedEmailDomain : BaseModel
    {
        public int ProhibitiedEmailDomainId { get; set; }

        public Guid ProhibitiedEmailDomainGuid { get; set; }

        [StringLength(500)]
        public string Value { get; set; }
    }
}
