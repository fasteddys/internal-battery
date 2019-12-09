using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpDiddyApi.Models
{
    public enum CourseSchedules { OnDemand = 0, InstrunctorLed };

    public class Course : BaseModel
    {
        public int CourseId { get; set; }
        public Guid? CourseGuid { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Slug { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public int? TopicId { get; set; }
        public int? TopicSecondaryId { get; set; }
        public string DesktopImage { get; set; }
        public string TabletImage { get; set; }
        public string MobileImage { get; set; }
        public int VendorId { get; set; }
        public Vendor Vendor { get; set; }
        public int? SortOrder { get; set; }
        public int? CourseDeliveryId { get; set; }
        public int? Hidden { get; set; }
        public string VideoUrl { get; set; }
        public List<CourseVariant> CourseVariants { get; set; }
        public List<CourseSkill> CourseSkills { get; set; }
        public bool IsExternal { get; set; }
        public CourseLevel CourseLevel { get; set; }
        public int? CourseLevelId { get; set; }
    }

    [NotMapped]
    public class CourseParams
    {
        public int? CourseId { get; set; }
        public Guid CourseVariantTypeGuid { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool IsExternal { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public List<Guid> SkillGuids { get; set; } = new List<Guid>();
        public List<Guid> TagGuids { get; set; } = new List<Guid>();
        public Guid VendorGuid { get; set; }
    }
}