using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class CountryDto 
    {
        public Guid CountryGuid { get; set; }
        public string Code3 { get; set; }
        public string DisplayName { get; set; }
        public int Sequence { get; set; }
    }
}
