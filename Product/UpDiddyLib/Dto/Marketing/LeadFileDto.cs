using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UpDiddyLib.Dto.Marketing
{
    public class LeadFileDto
    {
        [Required]
        public string ApiToken { get; set; }
        [Required]
        public Guid LeadIdentifier { get; set; }
        [Required]
        public IFormFile File { get; set; }
    }
}
