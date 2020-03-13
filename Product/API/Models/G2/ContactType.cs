using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.G2
{
    [Table("ContactTypes", Schema = "G2")]
    public class ContactType : BaseModel
    {
        public int ContactTypeId { get; set; }
        public Guid ContactTypeGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Sequence { get; set; }
    }
}
