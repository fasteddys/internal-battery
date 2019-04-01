using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class OfferDto : BaseDto
    {
        public int OfferId { get; set; }
        public Guid OfferGuid { get; set; }
        public int PartnerId { get; set; }
        public PartnerDto Partner { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Disclaimer { get; set; }
        public string Code { get; set; }
        public string Url { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
