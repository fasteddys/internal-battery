using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class Offer : BaseModel
    {
        public int OfferId { get; set; }
        [Required]
        public Guid OfferGuid { get; set; }
        [Required]
        public int PartnerId { get; set; }
        public virtual Partner Partner { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Disclaimer { get; set; }
        public string Code { get; set; }
        public string Url { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
