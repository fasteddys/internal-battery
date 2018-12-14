using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    class ResumeDto : BaseDto
    {
        public IFormFile Resume { get; set; } 
    }
}
