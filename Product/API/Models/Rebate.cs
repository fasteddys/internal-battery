using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class Rebate
    {
        public int RebateId { get; set; }
        public Guid? RebateGuid { get; set; }
        public int EnrollmentId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public Decimal RebateAmount { get; set; }
        public int RebateIssued { get; set; }
        public DateTime RebateIssueDate { get; set; }
        public int RebateIssueStatus { get; set; }

    }
}
