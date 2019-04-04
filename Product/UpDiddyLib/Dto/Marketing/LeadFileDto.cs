using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UpDiddyLib.Dto.Marketing
{
    public class LeadFileDto
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "MimeType is required")]
        public string MimeType { get; set; }
        [Required(ErrorMessage = "Base64EncodedData is required")]
        public string Base64EncodedData { get; set; }
    }
}
