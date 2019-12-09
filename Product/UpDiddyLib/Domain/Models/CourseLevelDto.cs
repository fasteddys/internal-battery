using System;
namespace UpDiddyLib.Domain.Models
{
    public class CourseLevelDto
    {
        public Guid CourseLevelGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
    }
}