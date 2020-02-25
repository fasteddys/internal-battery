using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.G2
{
    [Table("ProfileTags", Schema = "G2")]
    public class ProfileTag : BaseModel
    {
        public int ProfileTagId { get; set; }
        public Guid ProfileTagGuid { get; set; }
        public int ProfileId { get; set; }
        public virtual Profile Profile { get; set; }
        public int TagId { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
