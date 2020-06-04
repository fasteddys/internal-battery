using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.G2
{
    [Table("ReferenceCheckReport", Schema = "G2")]
    public class ReferenceCheckReport : BaseModel
    {
        public int ReferenceCheckReportId { get; set; }

        public Guid ReferenceCheckReportGuid { get; set; }

        /// <summary>
        /// Base64 string representation of report PDF file.
        /// File is assumed to be a PDF all the time.
        /// </summary>
        [Column(TypeName = "Varchar(MAX)")]
        public string Base64File { get; set; }

        /// <summary>
        /// Url to the reference check report file.
        /// </summary>
        [Required]
        [Url]
        public string FileUrl { get; set; }

        /// <summary>
        /// A hard coded field having two types for now Full or Summary
        /// </summary>
        [Required]
        [StringLength(25, MinimumLength = 1)]
        public string FileType { get; set; }

        public int ReferenceCheckId { get; set; }
        public virtual ReferenceCheck ReferenceCheck { get; set; }

    }
}
