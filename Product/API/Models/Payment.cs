using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public Guid? PaymentGuid { get; set; }
        public string PaymentNonce { get; set; }
        public Decimal PaymentValue { get; set; }
        public string PaymentCurrencyType { get; set; }
        public DateTime PaymentDate { get; set; }
        public int PaymentStatus { get; set; }
        public int PaymentProcessorId { get; set; }
        public Guid EnrollmentGuid { get; set; }
        public int PaymentBatchId { get; set; }
    }
}
