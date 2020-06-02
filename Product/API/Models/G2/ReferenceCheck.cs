using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.G2
{
    [Table("ReferenceCheck", Schema = "G2")]
    public class ReferenceCheck : BaseModel
    {
        public int ReferenceCheckId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ReferenceCheckGuid { get; set; }

        /// <summary>
        /// The request id tied to the vendor-webapi response when a request is made.
        /// </summary>
        public string ReferenceCheckRequestId { get; set; }

        /// <summary>
        /// Candidate's <see cref="Profile.ProfileId">ProfileId<>.
        /// </summary>
        [Required]
        public int ProfileId { get; set; }
        public virtual Profile Profile { get; set; }

        [Required]
        public int ReferenceCheckStatusId { get; set; }
        public virtual ReferenceCheckStatus ReferenceCheckStatus { get; set; }

        [Required]
        public int ReferenceCheckVendorId { get; set; }
        public virtual ReferenceCheckVendor ReferenceCheckVendor { get; set; }

        [Required]
        public int RecruiterId { get; set; }
        public virtual Recruiter Recruiter { get; set; }

    }
}
