using AutoMapper;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;

using UpDiddyApi.ApplicationCore.Exceptions;
namespace UpDiddyApi.ApplicationCore.Services
{
    public class CourseFavoriteService : ICourseFavoriteService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        public CourseFavoriteService(IRepositoryWrapper repositoryWrapper, IMapper mapper, IConfiguration configuration)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _config = configuration;
        }

        public async Task AddToFavorite(Guid subscriberGuid, Guid courseGuid)
        {
            if (subscriberGuid == null || subscriberGuid == Guid.Empty)
                throw new NullReferenceException("SubscriberGuid cannot be null");
            if (courseGuid == null || courseGuid == Guid.Empty)
                throw new NullReferenceException("CourseGuid cannot be null");
            var course = await _repositoryWrapper.Course.GetByGuid(courseGuid);
            if (course == null)
                throw new NotFoundException("Course not found");
            var subscriber = await _repositoryWrapper.SubscriberRepository.GetByGuid(subscriberGuid);
            if (subscriber == null)
                throw new NotFoundException("Subscriber not found");
            var courseFavorite = await _repositoryWrapper.CourseFavoriteRepository.GetBySubscriberGuidAndCourseGuid(subscriberGuid, courseGuid);
            if (courseFavorite == null)
            {
                courseFavorite = new CourseFavorite()
                {
                    Course = course,
                    CourseId = course.CourseId,
                    CreateDate = DateTime.UtcNow,
                    SubscriberId = subscriber.SubscriberId
                };
                await _repositoryWrapper.CourseFavoriteRepository.Create(courseFavorite);
                await _repositoryWrapper.SaveAsync();
            }
            else
            {
                if (courseFavorite.IsDeleted == 0)
                {
                    throw new AlreadyExistsException("Course already added to favorites");
                }
                else
                {
                    courseFavorite.IsDeleted = 0;
                    courseFavorite.ModifyDate = DateTime.UtcNow;
                    await _repositoryWrapper.SaveAsync();
                }
            }
        }

        public async Task RemoveFromFavorite(Guid subscriberGuid, Guid courseGuid)
        {
            if (subscriberGuid == null || subscriberGuid == Guid.Empty)
                throw new NullReferenceException("SubscriberGuid cannot be null");
            if (courseGuid == null || courseGuid == Guid.Empty)
                throw new NullReferenceException("CourseGuid cannot be null");
            var course = await _repositoryWrapper.Course.GetByGuid(courseGuid);
            if (course == null)
                throw new NotFoundException("Course not found");
            var subscriber = await _repositoryWrapper.SubscriberRepository.GetByGuid(subscriberGuid);
            if (subscriber == null)
                throw new NotFoundException("Subscriber not found");
            var courseFavorite = await _repositoryWrapper.CourseFavoriteRepository.GetBySubscriberGuidAndCourseGuid(subscriberGuid, courseGuid);
            if (courseFavorite == null)
            {
                throw new NotFoundException("Course has not been added to favorites");
            }
            else
            {
                courseFavorite.IsDeleted = 1;
                courseFavorite.ModifyDate = DateTime.UtcNow;
                await _repositoryWrapper.SaveAsync();
            }
        }

        public async Task<bool> IsCourseAddedToFavorite(Guid subscriberGuid, Guid courseGuid)
        {
            if (subscriberGuid == null || subscriberGuid == Guid.Empty)
                throw new NullReferenceException("SubscriberGuid cannot be null");
            if (courseGuid == null || courseGuid == Guid.Empty)
                throw new NullReferenceException("CourseGuid cannot be null");
            var courseFavorite = await _repositoryWrapper.CourseFavoriteRepository.GetBySubscriberGuidAndCourseGuid(subscriberGuid, courseGuid);
            if (courseFavorite != null)
            {
                if (courseFavorite.IsDeleted == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<List<CourseDetailDto>> GetFavoriteCourses(Guid subscriberGuid, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var courses = await _repositoryWrapper.StoredProcedureRepository.GetFavoriteCourses(subscriberGuid, limit, offset, sort, order);
            if (courses == null)
                throw new NotFoundException("Courses not found");
            return (courses);
        }
    }
}
