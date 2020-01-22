using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{

    public class GroupInfoListDto
    {
        public List<GroupInfoDto> Entities { get; set; }
        public int TotalRecords { get; set; }
    }


    public class GroupInfoDto
    {
        [JsonIgnore]
        public int TotalRecords { get; set; }
        public Guid GroupGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public int IsLeavable { get; set; }
    }
}

