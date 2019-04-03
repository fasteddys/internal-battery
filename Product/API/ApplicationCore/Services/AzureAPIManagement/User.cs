using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Services.AzureAPIManagement
{
    public class User
    {
        public string Id;
        public string FirstName;
        public string LastName;
        public string Email;
        public string State;
        public DateTime? RegistrationDate;

        public string GetUserId()
        {
            return Id.Split('/').LastOrDefault();
        }

        public string FullName {
            get {
                string[] names = { FirstName, LastName };
                return String.Join(" ", names.Where(s => !String.IsNullOrEmpty(s)));
            }
        }
    }
}
