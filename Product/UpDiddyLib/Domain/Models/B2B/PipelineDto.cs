using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UpDiddyLib.Domain.Models.B2B
{
    public class PipelineListDto
    {
        public List<PipelineDto> Pipelines { get; set; } = new List<PipelineDto>();
        public int TotalRecords { get; set; }
    }

    public class PipelineDto
    {
        public Guid PipelineGuid { get; set; }
        public Guid HiringManagerGuid { get; set; }
        [Required]
        [StringLength(25)]
        public string Name { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}
