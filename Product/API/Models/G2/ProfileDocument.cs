using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.G2
{
    [Table("ProfileDocuments", Schema = "G2")]
    public class ProfileDocument : BaseModel
    {
        public int ProfileDocumentId { get; set; }
        public Guid ProfileDocumentGuid { get; set; }
        [Required]
        [StringLength(100)]
        public string BlobStorageUrl { get; set; }
    }
}
