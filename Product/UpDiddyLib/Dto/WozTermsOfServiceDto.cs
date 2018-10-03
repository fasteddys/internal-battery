using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class WozTermsOfServiceDto : BaseDto
    {

        public int WozTermsOfServiceId { get; set; }
        public int DocumentId {get; set;}
        public string TermsOfService { get; set; }
    }
}
