using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class CoursePromoCode
    {
        public int CoursePromoCodeId { get; set; }
        public Guid? CoursePromoCodeGuid { get; set; }
        public int CourseId { get; set; }
        public int PromoCodeId { get; set; }
    }
}
