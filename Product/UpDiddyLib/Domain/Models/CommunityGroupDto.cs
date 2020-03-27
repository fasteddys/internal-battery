using System;
using System.ComponentModel.DataAnnotations;

namespace UpDiddyLib.Domain.Models
{
    public class CommunityGroupDto
    {
        public string Name { get; set; }

        public Guid CommunityGroupGuid { get; set; }
    }
}
