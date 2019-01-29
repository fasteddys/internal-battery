using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
namespace UpDiddyApi.Models
{
    public class CourseVariant : BaseModel
    {
        public int CourseVariantId { get; set; }
        public Guid? CourseVariantGuid { get; set; }
        [Column(TypeName="decimal(18,2)")]
        public Decimal Price { get; set; }
        public int CourseVariantTypeId { get; set; }
        public virtual CourseVariantType CourseVariantType { get; set; }
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }
    }
}