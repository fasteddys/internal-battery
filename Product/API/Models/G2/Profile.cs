using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.G2
{
    [Table("Profiles", Schema = "G2")]
    public class Profile : BaseModel
    {
        public int ProfileId { get; set; }
        public Guid ProfileGuid { get; set; }
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
        public int SubscriberId { get; set; }
        public virtual Subscriber Subscriber { get; set; }
        [StringLength(100)]
        public string FirstName { get; set; }
        [StringLength(100)]
        public string LastName { get; set; }
        [StringLength(254)]
        public string Email { get; set; }
        [StringLength(20)]
        public string PhoneNumber { get; set; }
        public int? ContactTypeId { get; set; }
        public virtual ContactType ContactType { get; set; }
        [StringLength(100)]
        public string StreetAddress { get; set; }
        public int? CityId { get; set; }
        public virtual City City { get; set; }
        public int? StateId { get; set; }
        public virtual State State { get; set; }
        public int? PostalId { get; set; }
        public virtual Postal Postal { get; set; }
        public int? ExperienceLevelId { get; set; }
        public virtual ExperienceLevel ExperienceLevel { get; set; }
        [StringLength(100)]
        public string Title { get; set; }
        public bool? IsWillingToTravel { get; set; }
        public bool? IsWillingToRelocate { get; set; }
        public bool? IsActiveJobSeeker { get; set; }
        public bool? IsCurrentlyEmployed { get; set; }
        public bool? IsWillingToWorkProBono { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? CurrentRate { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DesiredRate { get; set; }
        [StringLength(500)]
        public string Goals { get; set; }
        [StringLength(500)]
        public string Preferences { get; set; }
        [StringLength(500)]
        public string SkillsNote { get; set; }
        public int? AzureIndexStatusId { get; set; }
        public AzureIndexStatus AzureIndexStatus { get; set; }
        public string AzureSearchIndexInfo { get; set; }
        public List<ProfileEmploymentType> ProfileEmploymentTypes { get; set; }
    }
}
