using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class EnrollmentLog : BaseModel
    {
        public int EnrollmentLogId { get; set; }
        [Required]
        public Guid EnrollmentLogGuid { get; set; }
        [Required]
        public DateTime EnrollmentTime { get; set; }
        [Required]
        public Guid SubscriberGuid { get; set; }
        [Required]
        public Guid CourseGuid { get; set; }
        [Required]
        public Guid EnrollmentGuid { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public Decimal CourseCost { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public Decimal PromoApplied { get; set; }
        [Required]
        public int EnrollmentVendorPaymentStatusId { get; set; }
        [Required]
        public int EnrollmentVendorInvoicePaymentYear { get; set; }
        [Required]
        public int EnrollmentVendorInvoicePaymentMonth { get; set; }
        public Guid? CourseVariantGuid { get; set; }
    }
}
