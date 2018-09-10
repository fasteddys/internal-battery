using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UpDiddyApi.Models
{
    public class Subscriber
    {
        public int SubscriberId { get; set; }         
        // Azure ADB2C id for the user 
        [Required]
        public string MsalObjectId { get; set; }        
        public int IsDeleted { get; set; }
    }
}