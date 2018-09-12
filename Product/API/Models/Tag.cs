using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class Tag : BaseModel
    {
        public int TagId { get; set; }
        public Guid? TagGuid { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
