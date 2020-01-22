using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
   public interface IGroupService
   {
        Task<GroupInfoListDto> GetGroups(int limit, int offset, string sort, string order);
    }
}
