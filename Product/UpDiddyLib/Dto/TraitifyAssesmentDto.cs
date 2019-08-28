
using System.Collections.Generic;
namespace UpDiddyLib.Dto
{
    public class TraitifyAssesmentDto
    {
      public string AssesmentId { get; set; }
      public List<TraitifySlideDto> Slides { get; set; }
    }
}