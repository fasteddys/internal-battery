using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models
{
    public class EmploymentTypeListDto
    {
        public List<EmploymentTypeDto> EmploymentTypes { get; set; } = new List<EmploymentTypeDto>();
        public int TotalRecords { get; set; }
    }

    public class EmploymentTypeDto
    {
        public Guid EmploymentTypeGuid { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}