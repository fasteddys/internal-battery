using System.Collections.Generic;

namespace UpDiddyApi.ApplicationCore.Services.B2B.CareerTalentPipeline
{
    public class CareerTalentPipelineOptions
    {
        public string ccEmail { get; set; }

        public string ResponseEmailTemplateId { get; set; }

        public string ccEmailTemplateId { get; set; }

        public List<string> Questions { get; set; } = new List<string>();
    }
}
