using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class Topic : BaseModel
    {
        public int TopicId { get; set; }
        public Guid? TopicGuid { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public string DesktopImage { get; set; }
        public string TabletImage { get; set; }
        public string MobileImage { get; set; }
        public int? SortOrder { get; set; }
    }
}
