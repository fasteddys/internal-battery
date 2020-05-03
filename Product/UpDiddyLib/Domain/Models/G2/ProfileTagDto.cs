using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UpDiddyLib.Domain.Models.G2
{
    public class ProfileTagListDto
    {
        public List<ProfileTagDto> ProfileTags { get; set; } = new List<ProfileTagDto>();
        public int TotalRecords { get; set; }
    }

    public class ProfileTagDto
    {
        public Guid ProfileTagGuid { get; set; }
        public Guid ProfileGuid { get; set; }
        public Guid TagGuid { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}
