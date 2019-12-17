using System;
namespace UpDiddyLib.Domain.Models
{
    public class OfferDto
    {
        public Guid OfferGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Disclaimer { get; set; }
        public string Code { get; set; }
        public string Url { get; set; }
        public Guid? PartnerGuid {get;set;}
        public string PartnerLogoUrl { get; set; }
        public string PartnerName { get; set; }
        public DateTime startDate {get;set;}
        public DateTime? endDate {get;set;}
    }
}