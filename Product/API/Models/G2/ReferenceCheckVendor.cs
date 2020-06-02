using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.G2
{
    [Table("ReferenceCheckVendor", Schema = "G2")]
    public class ReferenceCheckVendor : BaseModel
    {
        public int ReferenceCheckVendorId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ReferenceCheckVendorGuid { get; set; }

        [StringLength(50)]
        public string Name { get; set; } //CrossChq

        [StringLength(250)]
        public string Description { get; set; } //https://crosschq.com


    }
}
