using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
namespace UpDiddyApi.ApplicationCore.Factory
{
    public class CourseVariantFactory 
    {

        public static async Task<CourseVariant> GetCourseVariantById(IRepositoryWrapper repositoryWrapper, int courseVariantId)
        {
            return await repositoryWrapper.CourseVariant.GetAll()
                .Where(s => s.IsDeleted == 0 && s.CourseVariantId == courseVariantId)
                .FirstOrDefaultAsync();
        }

        public static async Task<string> GetCourseVariantCourseSlug(IRepositoryWrapper repositoryWrapper, int courseVariantId)
        {

            var newObj = repositoryWrapper.CourseVariant.GetAll()
               .Join(repositoryWrapper.Course.GetAll(),
                   courseVariant => courseVariant.CourseId,
                   course => course.CourseId,
                   (courseVariant, course) => new { course, courseVariant })
                .Where(x => x.courseVariant.CourseVariantId == courseVariantId)
                .Select(m => new { slug = m.course.Slug });

            string courseSlug = await newObj.Select(n => n.slug).FirstOrDefaultAsync();
            return courseSlug;
        }


    }
}
