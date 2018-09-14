using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class EnrollmentStatus
    {
        public int EnrollmentStatusId { get; set; }
        public Guid? EnrollmentStatusGuid { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
