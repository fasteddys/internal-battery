using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class TrainingTypesDto
    {
        public List<TrainingTypeDto> TrainingTypes { get; set; } = new List<TrainingTypeDto>();
        public int TotalRecords { get; set; }
    }
}
