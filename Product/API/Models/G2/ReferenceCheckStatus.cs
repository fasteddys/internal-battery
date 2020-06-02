using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.G2
{
    [Table("ReferenceCheckStatus", Schema = "G2")]
    public class ReferenceCheckStatus : BaseModel
    {
        public int ReferenceCheckStatusId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ReferenceCheckStatusGuid { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Name { get; set; }

        [StringLength(250)]
        public string Description { get; set; }
    }
}
