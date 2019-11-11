using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IResumeService
    {
      Task UploadResume(Guid subscriberGuid, IFormFile resumeDoc);
      Task<FileDto> DownloadResume(Guid subscriberGuid);
    }
}
