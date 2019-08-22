using System;
namespace UpDiddyApi.Models
{
    public class JobCountPerProvince
    {
        public string Province { get; set; }
        public string CompanyName { get ; set; }
        public Guid CompanyGuid { get; set; }
        public int Count { get; set; }    
    }
}