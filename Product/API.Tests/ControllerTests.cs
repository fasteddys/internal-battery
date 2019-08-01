using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Controllers;
using UpDiddyApi.Helpers;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
using Xunit;

namespace API.Tests
{
    public class ControllerTests
    {
        /* Notes:
         * It will be difficult to unit test CourseController.GetCourse because it uses the Woz interface... use DI to make it testable?
         * Need to create helper method to encapsulate common setup operations (using Moq) once all dependencies are well known
         * Discovered a potential issue with AutoMapper... it appears that mappings with related entities can cause a problem if the related member property is null
         */

        [Fact]
        public void Course_Does_Not_Return_Deleted_Entity()
        {
            #region refactor this once we have multiple tests that require these objects
            var configuration = new Mock<IConfigurationRoot>();
            configuration.SetupGet(x => x[It.IsAny<string>()]).Returns("any value (not used right now)");
            var dbContextOptions = new DbContextOptionsBuilder<UpDiddyDbContext>()
                .UseInMemoryDatabase(databaseName: "careerCircleTestDb")
                .Options;
            var mapper = new Mapper(
                new MapperConfiguration(configure =>
                {
                    configure.AddProfile<ApiProfile>();
                })
            );
            var email = new Mock<SysEmail>(configuration.Object);
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var courseLog = new Mock<ILogger<CourseController>>();
            var topicLog = new Mock<ILogger<TopicController>>();
            var cache = new Mock<IDistributedCache>();
            var hangfireService = new Mock<IHangfireService>();
            #endregion

            using (var db = new UpDiddyDbContext(dbContextOptions))
            {
                // arrange
                var courseVariants = new List<CourseVariant>();
                courseVariants.Add(new CourseVariant() { Price = 1460, IsDeleted = 0, CourseVariantType = new CourseVariantType() { IsDeleted = 0, Name = "Instructor-Led" } });
                courseVariants.Add(new CourseVariant() { Price = 249, IsDeleted = 0, CourseVariantType = new CourseVariantType() { IsDeleted = 0, Name = "Self-Paced" } });
                var vendor = new Vendor() { IsDeleted = 0, Name = "WozU" };
                db.Add(new Course() { Name = "CourseA (deleted)", IsDeleted = 1, CourseVariants = courseVariants, Vendor = vendor });
                db.Add(new Course() { Name = "CourseB (active)", IsDeleted = 0, CourseVariants = courseVariants, Vendor = vendor });
                db.Add(new Course() { Name = "CourseC (active)", IsDeleted = 0, CourseVariants = courseVariants, Vendor = vendor });
                db.SaveChanges();
                var courseController = new CourseController(db, mapper, configuration.Object, email.Object, httpClientFactory.Object, courseLog.Object, cache.Object, hangfireService.Object);

                // act
                List<CourseDto> result = null;
                var response = courseController.Get();
                try
                {
                    result = (List<CourseDto>)(((OkObjectResult)response).Value);
                }
                catch (Exception) { }

                // assert                
                Assert.IsType<OkObjectResult>(response);
                Assert.NotNull(result);
                Assert.Equal(2, result.Count);
            }
        }
    }
}
