using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ICourseEnrollmentService
    {
        Task<CourseCheckoutDto> GetCourseCheckoutInfo(Guid subscriberGuid, string courseSlug);
        Task<Guid> Enroll(Guid subscriberGuid, EnrollmentFlowDto EnrollmentFlowDto);

    }
}
