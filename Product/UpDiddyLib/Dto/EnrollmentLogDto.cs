using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class EnrollmentLogDto : BaseDto
    {
        public int EnrollmentLogId { get; set; }
        public Guid EnrollmentLogGuid { get; set; }
        public DateTime EnrollmentTime { get; set; }
        public Guid SubscriberGuid { get; set; }
        public Guid CourseGuid { get; set; }
        public Guid EnrollmentGuid { get; set; }
        public Decimal CourseCost { get; set; }
        public Decimal PromoApplied { get; set; }
        public int EnrollmentVendorPaymentStatusId { get; set; }
        public int EnrollmentVendorInvoicePaymentYear { get; set; }
        public int EnrollmentVendorInvoicePaymentMonth { get; set; }
    }
}