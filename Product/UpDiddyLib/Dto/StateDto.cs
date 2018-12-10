using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class StateDto
    {
        public int StateId { get; set; }
        public Guid? StateGuid { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Sequence { get; set; }
        public CountryDto Country { get; set; }
    }
}
