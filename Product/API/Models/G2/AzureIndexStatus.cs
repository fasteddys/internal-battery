using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.G2
{
    [Table("AzureIndexStatuses", Schema = "G2")]
    public class AzureIndexStatus : BaseModel
    {
        public int AzureIndexStatusId { get; set; }
        public Guid AzureIndexStatusGuid { get; set; }
        [Required]
        [StringLength(25)]
        public string Name { get; set; }
        [StringLength(100)]
        public string Description { get; set; }
    }
}
