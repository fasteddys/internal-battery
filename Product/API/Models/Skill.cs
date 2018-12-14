using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public partial class Skill : BaseModel
    {
        public int SkillId { get; set; }
        public string SkillName { get; set; }
        public string ONetCode { get; set; }
    }
}
