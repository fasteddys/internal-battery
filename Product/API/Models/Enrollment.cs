using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UpDiddyApi.Models
{
    public class Enrollment
    {
        public int EnrollmentId { get; set; }         
        // Azure ADB2C id for the user 
        [Required]
        public int CourseId { get; set; }

        public int SubscriberId { get; set; }

        public DateTime EnrollDate { get; set; }
        public Decimal  EnrollmentFee { get; set; }

        public int IsDeleted { get; set; }
    }
}