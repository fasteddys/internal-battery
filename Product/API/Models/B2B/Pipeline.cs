using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.B2B
{
    [Table("Pipelines", Schema = "B2B")]
    public class Pipeline : BaseModel
    {
        public int PipelineId { get; set; }
        public Guid PipelineGuid { get; set; }
        public int HiringManagerId { get; set; }
        public virtual HiringManager HiringManager { get; set; }
        [Required]
        [StringLength(25)]
        public string Name { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
    }
}