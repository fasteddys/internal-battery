using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class LinkedInProfileDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName  { get; set; }
        public string MaidenName { get; set; }
        public string FormattedName { get; set; }
        public string PhoneticFirstName { get; set; }
        public string PhoneticLastName { get; set; }
        public string FormattedPhoneticName { get; set; }
        public string Location { get; set; }
        public string LocationCountry { get; set; }
        public string Industry { get; set; }
        public string CurrentShare{ get; set; }
        public int NumConections { get; set; }
        public int NumConnectionsCapped { get; set; }
        public string Summary { get; set; }
        public string Specialties { get; set; }
        public IList<LinkedInPositionDto> Positions { get; set; }        
        public string PictureUrl{ get; set; }
        public string SiteStandardProfileRequest { get; set; }
        public string ApiStandardProfileRequest { get; set; }
        public string PublicProfileUrl   { get; set; } 
    }
}
