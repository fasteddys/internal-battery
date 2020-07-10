using System;
using System.Collections.Generic;
using System.Text;
using UpDiddyLib.Dto;

namespace UpDiddyLib.Domain.Models
{
    public class CommuteDistancesDto
    {
        public List<CommuteDistanceDto> CommuteDistances { get; set; }
        public int TotalRecords { get; set; }
    }

    public class CommuteDistanceDto
    {
        public Guid CommuteDistanceGuid { get; set; }
        public string Name { get; set; }
    }
}
