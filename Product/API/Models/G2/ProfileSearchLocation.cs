using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.G2
{
    [Table("ProfileSearchLocations", Schema = "G2")]
    public class ProfileSearchLocation : BaseModel
    {
        public int ProfileSearchLocationId { get; set; }
        public Guid ProfileSearchLocationGuid { get; set; }
        public int ProfileId { get; set; }
        public virtual Profile Profile { get; set; }
        public int? CityId { get; set; }
        public virtual City City { get; set; }
        public int? StateId { get; set; }
        public virtual State State { get; set; }
        public int? PostalId { get; set; }
        public virtual Postal Postal { get; set; }
        [DefaultValue(25)]
        public int SearchRadius { get; set; }
    }
}
