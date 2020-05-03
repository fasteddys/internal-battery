using UpDiddyApi.Models;
using UpDiddyApi.Models.G2;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class HydratedEmailTemplate
    {
        public string Value { get; set; }
        public Recruiter Recruiter { get; set; }
        public Profile Profile { get; set; }
    }
}
