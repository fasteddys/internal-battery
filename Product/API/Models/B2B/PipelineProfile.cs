using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models.G2;

namespace UpDiddyApi.Models.B2B
{
    [Table("PipelineProfiles", Schema = "B2B")]
    public class PipelineProfile : BaseModel
    {

        public int PipelineProfileId { get; set; }
        public Guid PipelineProfileGuid { get; set; }
        public int PipelineId { get; set; }
        public virtual Pipeline Pipeline { get; set; }
        public int ProfileId { get; set; }
        public virtual Profile Profile { get; set; }
    }
}
