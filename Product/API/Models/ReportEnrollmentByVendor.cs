using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class ReportEnrollmentByVendor
    {
        public int ReportEnrollmentByVendorId { get; set; }
        public Guid? ReportEnrollmentByVendorGuid { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int EnrollmentsCompleted { get; set; }
        public int VendorId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public Decimal TotalRevenueIn { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public Decimal TotalSplitOut { get; set; }
        public int CoursesCompletedCount { get; set; }
        public int CoursesDroppedCount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public Decimal TotalRefundsIssued { get; set; }
        public int PromoCodesRedeemedCount { get; set; }
    }
}
