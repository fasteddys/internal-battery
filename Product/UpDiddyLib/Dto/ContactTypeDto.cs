using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UpDiddyLib.Dto
{
    public class ContactTypeListDto
    {
        public List<ContactTypeDto> ContactTypes { get; set; } = new List<ContactTypeDto>();
        public int TotalRecords => ContactTypes?.Count ?? 0;
    }

    public class ContactTypeDto
    {
        public Guid ContactTypeGuid { get; set; }
        public string Name { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Description { get; set; }
    }
}
