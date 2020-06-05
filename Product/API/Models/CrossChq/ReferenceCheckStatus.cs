using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.CrossChq
{
    [Table("ReferenceCheckStatus", Schema = "G2")]
    public class ReferenceCheckStatus : BaseModel
    {
        public int ReferenceCheckStatusId { get; set; }

        public Guid ReferenceCheckStatusGuid { get; set; }

        public int ReferenceCheckId { get; set; }
        public virtual ReferenceCheck ReferenceCheck { get; set; }

        /// <summary>
        /// An integer between 0-100 representing a percentage(%).
        /// </summary>
        public int  Progress { get; set; }

        /// <summary>
        /// A status value sent by the vendor.
        /// </summary>
        [StringLength(50, MinimumLength = 1)]
        public string Status { get; set; }

        /// <summary>
        /// Saves the web-hook json response.
        /// </summary>
        [StringLength(4000, MinimumLength = 1)]
        public string VendorJsonResponse { get; set; }

    }
}
