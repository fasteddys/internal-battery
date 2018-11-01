using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace UpDiddyApi.Models
{
    public class CourseVariant : BaseModel
    {
        public int CourseVariantId { get; set; }
        public Guid? CourseVariantGuid { get; set; }
        public Guid? CourseGuid { get; set; }
        public Decimal? Price { get; set; }
        public string VariantType { get; set; }
    }
}