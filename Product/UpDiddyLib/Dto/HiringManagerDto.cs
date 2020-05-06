using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class HiringManagerDto
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Title { get; set; }

        public string PhoneNumber { get; set; }

        public CompanyDto Company { get; set; }

    }
}
