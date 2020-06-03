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

        public Guid ReferenceCheckGuid { get; set; }

        /// <summary>
        /// The request id tied to the vendor-webapi response when a request is made.
        /// </summary>
        [StringLength(150, MinimumLength = 1)]
        public string ReferenceCheckRequestId { get; set; }

        /// <summary>
        /// The UTC datetime of the reference check when concluded.
        /// If null the reference check is incomplete.
        /// </summary>
        public DateTime? ReferenceCheckConcludedDate { get; set; }

        /// <summary>
        /// Type of reference check type, for example, the job_role json property that is returned by CrossChq API response.
        /// </summary>
        public string ReferenceCheckType { get; set; }

        /// <summary>
        /// Base64 string representation of PDF file.
        /// File is assumed to be a PDF all the time.
        /// </summary>
        [Column(TypeName = "Varchar(MAX)")]
        public string ReferenceCheckReportFile { get; set; }

        /// <summary>
        /// Url to the reference check report file.
        /// </summary>
        [Url]
        public string ReferenceCheckReportFileUrl { get; set; }

        [Required]
        [StringLength(75, MinimumLength = 1)]
        public string CandidateJobTitle { get; set; }

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

        public int CandidateReferenceId { get; set; }
        public virtual CandidateReference CandidateReference { get; set; }

    }
}
