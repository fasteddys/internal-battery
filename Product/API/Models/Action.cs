using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class Action : BaseModel
    {
        public int ActionId { get; set; }
        [Required]
        public Guid ActionGuid { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
