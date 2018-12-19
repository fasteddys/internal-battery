using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class ResumeDto : BaseDto
    {
        public Guid SubscriberGuid { get; set; }
        public string Base64EncodedResume { get; set; }
    }
}
