using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IResumeService
    {
      Task UploadResume(Guid subscriberGuid, FileDto fileDto);
      Task<FileDto> DownloadResume(Guid subscriberGuid);
    }
}
