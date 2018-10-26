using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class CountryDto : BaseDto
    {
        public int CountryId { get; set; }
        public Guid CountryGuid { get; set; }
        public string Code2 { get; set; }
        public string Code3 { get; set; }
        public string OfficialName { get; set; }

        private string _DisplayName;

        public string DisplayName
        {
            get { return (_DisplayName == null ? "" : _DisplayName); }
            set { _DisplayName = value; }
        }

    }
}
