using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UpDiddyLib.Domain.Models.G2
{
    public class CommentsDto
    {
        public List<Guid> ProfileGuids { get; set; }

        public Guid RecruiterGuid { get; set; }

        [Required]
        [StringLength(500)]
        public string Value { get; set; }

        public bool IsVisibleToCompany { get; set; }

        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}
