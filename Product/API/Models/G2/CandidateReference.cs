using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.G2
{
    [Table("CandidateReference", Schema = "G2")]
    public class CandidateReference : BaseModel
    {
        public int CandidateReferenceId { get; set; }

        public Guid CandidateReferenceGuid { get; set; }

        public int ReferenceCheckId { get; set; }
        public virtual ReferenceCheck ReferenceCheck { get; set; }

        [StringLength(100, MinimumLength = 1)]
        public string FirstName { get; set; }

        [StringLength(100, MinimumLength = 1)]
        public string LastName { get; set; }

        [StringLength(254, MinimumLength = 1)]
        public string Email { get; set; }

        [StringLength(20, MinimumLength = 1)]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// A status value sent by the vendor.
        /// </summary>
        [StringLength(50, MinimumLength = 1)]
        public string Status { get; set; }


    }
}
