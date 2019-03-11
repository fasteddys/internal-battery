using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class ImportValidationSummaryDto
    {
        public string ErrorMessage { get; set; }
        public string CacheKey { get; set; }
        public List<ImportActionDto> ImportActions { get; set; } = new List<ImportActionDto>();
        public List<ContactDto> ContactsPreview { get; set; } = new List<ContactDto>();
    }
}
