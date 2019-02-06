using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class CourseVariantFactory 
    {

        public static CourseVariant GetCourseVariantById(UpDiddyDbContext db, int courseVariantId)
        {
            return db.CourseVariant
                .Where(s => s.IsDeleted == 0 && s.CourseVariantId == courseVariantId)
                .FirstOrDefault();
        }

        public static string GetCourseVariantCourseSlug(UpDiddyDbContext db, int courseVariantId)
        {

            var newObj = db.CourseVariant
               .Join(db.Course,
                   courseVariant => courseVariant.CourseId,
                   course => course.CourseId,
                   (courseVariant, course) => new { course, courseVariant })
                .Where(x => x.courseVariant.CourseVariantId == courseVariantId)
                .Select(m => new { slug = m.course.Slug });

            string courseSlug = newObj.Select(n => n.slug).FirstOrDefault();
            return courseSlug;
        }


    }
}
