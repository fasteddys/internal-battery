using System;

namespace UpDiddyLib.Dto
{
    public class GroupDto : BaseDto
    {
        public Guid GroupGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public int IsLeavable { get; set; }
    }
}