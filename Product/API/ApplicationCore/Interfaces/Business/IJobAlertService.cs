using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IJobAlertService
    {
        Task<Guid> CreateJobAlert(Guid subscriberGuid, JobAlertDto jobAlertDto);
        Task<List<JobAlertDto>> GetJobAlert(Guid subscriberGuid);
        Task UpdateJobAlert(Guid subscriberGuid, Guid jobAlertGuid, JobAlertDto jobAlertDto);
        Task DeleteJobAlert(Guid subscriberGuid, Guid jobAlertGuid);
    }
}