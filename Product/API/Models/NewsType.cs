using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class NewsType : BaseModel
    {
        public int NewsTypeId { get; set; }
        public Guid? NewsTypeGuid { get; set; }
        [Required]
        public string NewsClassification { get; set; }
    }
}
