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

        [StringLength(100, MinimumLength = 1)]
        public string FirstName { get; set; }

        [StringLength(100, MinimumLength = 1)]
        public string LastName { get; set; }

        [StringLength(254, MinimumLength = 1)]
        public string Email { get; set; }

        [StringLength(20, MinimumLength = 1)]
        public string PhoneNumber { get; set; }


    }
}
