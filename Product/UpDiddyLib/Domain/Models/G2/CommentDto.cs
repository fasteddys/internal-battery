using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UpDiddyLib.Domain.Models.G2
{
    public class CommentListDto
    {
        public List<CommentDto> Comments { get; set; } = new List<CommentDto>();
        public int TotalRecords { get; set; }
    }

    public class CommentDto
    {
        public Guid CommentGuid { get; set; }
        public Guid RecruiterGuid { get; set; }
        public Guid ProfileGuid { get; set; }
        [Required]
        [StringLength(500)]
        public string Value { get; set; }
        public bool IsVisibleToCompany { get; set; }
        public DateTime CreateDate { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}
