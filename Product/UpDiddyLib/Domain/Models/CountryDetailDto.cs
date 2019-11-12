using System;
namespace UpDiddyLib.Domain.Models
{
    public class CountryDetailDto
    {
        public Guid CountryGuid {get;set;}
        public string DisplayName { get; set; }
        public string OfficialName { get; set; }
        public int Sequence { get; set; }
        public string Code2 { get; set; }
        public string Code3 { get; set; }
    }
}