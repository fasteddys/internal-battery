using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class JobQueryDto
    {

        /// <summary>
        /// Free format location string e.g 7312 parkway drive hanover md  OR 21204 , etc.  
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// List of keywords in posting e.g. skills, job description, commpany name etc.c 
        /// </summary>
        public string Keywords { get; set; }
        /// <summary>
        /// Single specific skill - To send more than one skill use keywords
        /// </summary>
        public string Skill { get; set; }
        /// <summary>
        /// specific country 
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        ///  specific state 
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// specific city 
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// specific street address 
        /// </summary>
        public string StreetAddress { get; set; }
        /// <summary>
        /// specific industry name 
        /// </summary>
        public string Industry { get; set; }
        /// <summary>
        /// specific job category name 
        /// </summary>
        public string  JobCategory { get; set; }
        /// <summary>
        /// date published values defined by cloud talent.  Valid values are PAST_24_HOURS, PAST_3_DAYS, PAST_WEEK, PAST_MONTH, PAST_YEAR 
        /// </summary>        
        public string DatePublished { get; set; }
        ///  specific company name 
        public string CompanyName { get; set; }
        /// <summary>
        ///  radius in miles from location
        /// </summary>
        public int DistanceInMiles { get; set; }
 
    }
}
