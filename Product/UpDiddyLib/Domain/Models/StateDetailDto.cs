using System;
namespace UpDiddyLib.Domain.Models
{
    public class StateDetailDto
    {
        public Guid StateGuid { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int Sequence { get; set; }
    }
}