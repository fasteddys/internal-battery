using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class RefundDto
    { 
        public Decimal RefundAmount { get; set; }
        public int RefundIssued { get; set; }
        public DateTime RefundIssueDate { get; set; }
        public int RefundIssueStatus { get; set; }
    }
}
