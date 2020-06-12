using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.G2
{
    [Table("ProfileComments", Schema = "G2")]
    public class ProfileComment : BaseModel
    {
        public int ProfileCommentId { get; set; }
        public Guid ProfileCommentGuid { get; set; }
        public int ProfileId { get; set; }
        public virtual Profile Profile { get; set; }
        public int RecruiterId { get; set; }
        public virtual Recruiter Recruiter { get; set; }
        [Required]
        [StringLength(2500)]
        public string Value { get; set; }
        [DefaultValue(false)]
        public bool IsVisibleToCompany { get; set; }
    }
}
