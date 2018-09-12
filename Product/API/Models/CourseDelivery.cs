using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class CourseDelivery : BaseModel
    {
        public int CourseDeliveryId { get; set; }
        public Guid? CourseDeliveryGuid { get; set; }
        public int CourseId { get; set; }
        [Required]
        public string DeliveryMethod { get; set; }
        public string DeliveryDescription { get; set; }
    }
}
