using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class Group : BaseModel
    {
        public int GroupId { get; set; }
        public Guid GroupGuid { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public int IsLeavable { get; set; }
    }
}
