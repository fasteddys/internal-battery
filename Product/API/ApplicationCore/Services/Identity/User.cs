using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Services.Identity
{
    public class User
    {
        public string UserId { get; set; }

        public Guid SubscriberGuid { get; set; }

        public bool? EmailVerified { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public ICollection<Role> Roles { get; set; } = new Collection<Role>();
    }
}
