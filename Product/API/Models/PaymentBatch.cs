using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class PaymentBatch
    {
        public int PaymentBatchId { get; set; }
        public Guid? PaymentBatchGuid { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }
}
