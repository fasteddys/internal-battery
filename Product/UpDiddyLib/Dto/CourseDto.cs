using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class CourseDto : BaseDto
    {
        public int CourseId { get; set; }
        public Guid? CourseGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public int TopicId { get; set; }
        public string DesktopImage { get; set; }
        public string TabletImage { get; set; }
        public string MobileImage { get; set; }
        public int? VendorId { get; set; }    
        public int? SortOrder { get; set; }
        public int? CourseDeliveryId { get; set; }
        public string Slug { get; set; }  
        public int? Hidden { get; set; }    
        public string VideoUrl { get; set; }
        public List<CourseVariantDto> CourseVariants { get; set; }
        public VendorDto Vendor { get; set; }
        // todo: does Woz-specific info belong somewhere else, should we use polymorphism to return diffent types based on vendor (or something else)?
        public int TermsOfServiceDocumentId { get; set; }
        public string TermsOfServiceContent { get; set; }
        public List<SkillDto> Skills { get; set; }
        public List<TagTopicDto> TagTopics { get; set; }
        public bool IsExternal { get; set; }
    }
}