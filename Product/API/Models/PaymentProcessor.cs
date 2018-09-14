using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class PaymentProcessor
    {
        public int PaymentProcessorId { get; set; }
        public Guid? PaymentProcessorGuid { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
