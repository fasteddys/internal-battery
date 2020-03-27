using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using UpDiddyApi.ApplicationCore.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpDiddyApi.Models
{
    public class CommunityGroup : BaseModel
    {
        public int CommunityGroupId { get; set; }
        public Guid? CommunityGroupGuid { get; set; }
        public string Name { get; set; }

        public string ExternalId { get; set; }
    }
}