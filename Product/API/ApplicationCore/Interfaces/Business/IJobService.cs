using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IJobService
    {
        Task ReferJobToFriend(JobReferralDto jobReferralDto);
        Task UpdateJobReferral(string referrerCode, string subscriberGuid);
        Task UpdateJobViewed(string referrerCode);
    }
}
