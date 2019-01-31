using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class Refund
    {
        public int RefundId { get; set; }
        public Guid? RefundGuid { get; set; }
        public int EnrollmentId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public Decimal RefundAmount { get; set; }
        public int RefundIssued { get; set; }
        public DateTime RefundIssueDate { get; set; }
        public int RefundIssueStatus { get; set; }
    }
}
