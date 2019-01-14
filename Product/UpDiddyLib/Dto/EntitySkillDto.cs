using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class EntitySkillDto : BaseDto
    {
        public List<SkillDto> Skills { get; set; }

        public Guid EntityGuid { get; set; }

        public string EntityType { get; set; }
    }
}
