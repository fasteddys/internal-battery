using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UpDiddyApi.Models
{
    public enum CourseSchedules { OnDemand = 0 , InstrunctorLed};

    public class Course
    {
        public int CourseId { get; set; } 
        
        // ID of the vendor of provides the course 
        [Required]
        public int VendorId { get; set; }

        // Vendor specific code for the couse 
        [Required]
        public string CourseCode { get; set; }

        public int CourseSchedule { get; set; }

        public Decimal  PurchasePrice6{ get; set; }

        public int IsDeleted { get; set; }
    }
}