using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
namespace UpDiddyApi.Models
{
    public class CourseVariant : BaseModel
    {
        public int CourseVariantId { get; set; }
        public Guid? CourseGuid { get; set; }
        public Decimal? Price { get; set; }
        [Required]
        public string VariantType { get; set; }

    }
}