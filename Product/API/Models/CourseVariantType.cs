using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class CourseVariantType : BaseModel
    {
        public int CourseVariantTypeId { get; set; }
        public Guid? CourseVariantGuid { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
