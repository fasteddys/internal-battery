using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models.CrossChq
{
    public class ReferenceStatusDto
    {
        public Guid ReferenceCheckId { get; set; }

        public string Status { get; set; }

        public string JobRole { get; set; }

        public string JobPosition { get; set; }

        public int PercentComplete { get; set; }

        public DateTime? CreateDate { get; set; }

        public List<ReferenceDto> References { get; set; }
    }
}
