using Newtonsoft.Json;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models.Reports
{
    public class UsersListDto
    {
        public List<UsersDto> NewUsers { get; set; } = new List<UsersDto>();
        public int TotalUsers { get; set; }
        public int TotalEnrollments { get; set; }
    }

    public class UsersDto
    {
        public string DateRange { get; set; }
        public int UsersCreated { get; set; }
        public int EnrollmentsCreated { get; set; }
        [JsonIgnore]
        public int TotalUsers { get; set; }
        [JsonIgnore]
        public int TotalEnrollments { get; set; }
    }
}
