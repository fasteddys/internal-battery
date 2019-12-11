using System;
namespace UpDiddyLib.Domain.Models
{
    public class CareerPathJobDto : JobBaseDto
    {
        public Guid JobPostingGuid { get; set; }
        public string Title { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
    }
}
