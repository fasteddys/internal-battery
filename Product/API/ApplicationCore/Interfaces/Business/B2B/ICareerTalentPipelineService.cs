﻿using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models.B2B;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business.B2B
{
    public interface ICareerTalentPipelineService
    {
        List<string> GetQuestions();

        Task<bool> SubmitCareerTalentPipeline(CareerTalentPipelineDto careerTalentPipelineDto);
    }
}
