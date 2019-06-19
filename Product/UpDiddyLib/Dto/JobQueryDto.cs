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

        /// <summary>
        /// Order of search results. Valid values are:
        /// 
        /// "relevance desc":                       By relevance descending, as determined by the API algorithms. Relevance thresholding of query results is only available with this ordering.
        ////"posting_publish_time desc":            By Job.posting_publish_time descending.
        ////"posting_update_time desc":             By Job.posting_update_time descending.
        ////"title":                                By Job.title ascending.
        ////"title desc":                           By Job.title descending.
        ////"annualized_base_compensation":         By job's CompensationInfo.annualized_base_compensation_range ascending. Jobs whose annualized base compensation is unspecified are put at the end of search results.
        ////"annualized_base_compensation desc":    By job's CompensationInfo.annualized_base_compensation_range descending. Jobs whose annualized base compensation is unspecified are put at the end of search results.
        ////"annualized_total_compensation":        By job's CompensationInfo.annualized_total_compensation_range ascending. Jobs whose annualized base compensation is unspecified are put at the end of search results.
        ////"annualized_total_compensation desc":   By job's CompensationInfo.annualized_total_compensation_range descending. Jobs whose annualized base compensation is unspecified are put at the end of search results.
                /// </summary>
        public string OrderBy { get; set; }


        #endregion

        #region Commute Search
        /// <summary>
        /// lattude of commute search - if both lat and long are non zero
        /// a commute search will be assumed
        /// </summary>
        public double Lat { get; set; }

        /// <summary>
        /// longitude of commute search - if both lat and long are non zero
        /// a commute search will be assumed
        /// </summary>
        public double Lng { get; set; }

        /// <summary>
        /// Only return jobs at are at most MaxCommuteTimeInMinutes from lat/lng 
        /// </summary>
        public int CommuteTime { get; set; }

        /// <summary>
        /// Set to true to only return jobs with fully qualified addresses 
        /// that include city, province and street addresses 
        /// </summary>
        public bool PreciseAddress { get; set; }

        /// <summary>
        /// Set to true if the commute method is public, otherwise private auto
        /// is assumed
        /// </summary>
        public bool PublicTransit { get; set; }


        /// <summary>
        /// Set to true if the commute time is during rush-hour, otherwhise a off hour 
        /// commmute will be assumed 
        /// is assumed
        /// </summary>
        public bool RushHour { get; set; }




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
        /// <summary>
        /// An optional parameter that can be used to set the TimestampRange parameter. UpperBound must also be set.
        /// </summary>
        public DateTime? LowerBound { get; set; }
        /// <summary>
        /// An optional parameter that can be used to set the TimestampRange parameter. LowerBound must also be set.
        /// </summary>
        public DateTime? UpperBound { get; set; }
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
