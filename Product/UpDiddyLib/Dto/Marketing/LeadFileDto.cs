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
        [Required(ErrorMessage = "LeadIdentifier is required")]
        public Guid LeadIdentifier { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "MimeType is required")]
        public string MimeType { get; set; }
        [Required(ErrorMessage = "Base64EncodedData is required")]
        public string Base64EncodedData { get; set; }
    }
}


/* consider doing something like this to validate the size of the file... use w/ MaxLength data annotation?
 
private Double calcBase64SizeInKBytes(String base64String) {
    Double result = -1.0;
    if(StringUtils.isNotEmpty(base64String)) {
        Integer padding = 0;
        if(base64String.endsWith("==")) {
            padding = 2;
        }
        else {
            if (base64String.endsWith("=")) padding = 1;
        }
        result = (Math.ceil(base64String.length() / 4) * 3 ) - padding;
    }
    return result / 1000;
}
 
     */
