using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpDiddyApi.Models
{
    public enum CourseSchedules { OnDemand = 0 , InstrunctorLed};

    public class Course : BaseModel
    { 
        public int CourseId { get; set; } 
        public Guid? CourseGuid { get; set; }
        [Required]
        public string CourseName { get; set; }
        public string CourseDescription { get; set; }
        public string CourseCode { get; set; }
        public Decimal? Price { get; set; }
        public int? TopicId { get; set; }
        public string DesktopImage { get; set; }
        public string TabletImage { get; set; }
        public string MobileImage { get; set; }
        public int? VendorId { get; set; }
        public int? SortOrder { get; set; }
        public int? CourseDeliveryId { get; set; }
        
    }
}