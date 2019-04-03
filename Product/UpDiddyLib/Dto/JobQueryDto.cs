using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class JobQueryDto
    {

        #region Pagination 

        /// <summary>
        /// number of jobs to return
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// page number of jobs 
        /// </summary>
        /// /// 
        public int PageNum { get; set; }
        
        /// <summary>
        /// number of pages 
        /// </summary>
        public int NumPages { get; set; }

        #endregion

        #region Search Options

        /// <summary>
        /// 
        /// Include facets in search results 
        /// </summary>
        public int ExcludeFacets { get; set; }

        /// <summary>
        /// Include custom job properties in search results 
        /// </summary>
        public int ExcludeCustomProperties { get; set; }


        #endregion



        #region Search Data
        /// <summary>
        /// Free format location string e.g 7312 parkway drive hanover md  OR 21204 , etc.  
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// name of the city of job posting 
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// state or provience of job posting 
        /// </summary>

        public string Province { get; set; }

        /// <summary>
        /// postal code of job posting 
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// street address of posting 
        /// </summary>
        public string StreetAddress { get; set; }

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
        public int SearchRadius { get; set; }
        /// <summary>
        /// employment type 
        /// </summary>
        public string EmploymentType { get; set; }
        /// <summary>
        /// experience level 
        /// </summary>
        public string ExperienceLevel { get; set; }
        /// <summary>
        /// /education level 
        /// </summary>
        public string EducationLevel{ get; set; }




        #endregion
    }
}
