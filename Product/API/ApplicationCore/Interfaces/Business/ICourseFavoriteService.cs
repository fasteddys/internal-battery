using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ICourseFavoriteService
    {
        Task<List<CourseFavoriteDto>> GetFavoriteCourses(Guid subscriberGuid,int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
        Task AddToFavorite(Guid subscriberGuid, Guid courseGuid);
        Task RemoveFromFavorite(Guid subscriberGuid, Guid courseGuid);
        Task<bool> IsCourseAddedToFavorite(Guid subscriberGuid, Guid courseGuid);
    }
}
