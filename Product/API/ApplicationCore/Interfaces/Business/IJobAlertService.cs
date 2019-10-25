using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IJobAlertService
    {
        Task CreateJobAlert(Guid subscriberGuid, JobAlertDto JobAlertDto);
        Task<List<JobAlertDto>> GetJobAlert(Guid subscriberGuid);
        Task DeleteJobAlert(Guid subscriberGuid, Guid jobAlertGuid);
    }
}
