using System;

namespace UpDiddyLib.Domain.Models
{
    //todo figure out why CourseVariantDetailDto can not inherit from CourseDetailDto.  If it it does the sproc
    // used to hydrate CourseVariantDetailDto's throw a "Cannot find column Discriminator" error.  Weird
    public class CourseVariantDetailDto
    {        
        public string VendorLogoUrl { get; set; }
        public Guid CourseGuid { get; set; }
        public string Title { get; set; }
        public string Duration { get; set; }
        public string Description { get; set; }
        public Guid? CourseLevelGuid { get; set; }
        public string VendorName { get; set; }
        public int NumEnrollments { get; set; }
        public Guid VendorGuid { get; set; }  
        public string Code { get; set; }
        public string Level { get; set; }
        public string Topic { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }
        public int IsDeleted { get; set; }
        public string CourseSkills { get; set; }
        public Guid CourseVariantGuid { get; set; }
        public Decimal Price { get; set; }
        public string CourseVariantType { get; set; }
        public string TabletImage { get; set; }
        public string DesktopImage { get; set; }
        public string MobileImage { get; set; }
        public string ThumbnailImage { get; set; }
    }
}
