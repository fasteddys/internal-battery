using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class ProfileQueryDto
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
 

        //TODO JAB Find doc on profile order by 
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

        #region location data 


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

        public int SearchRadius { get; set; }


        #endregion


        #region Search Data

        /// <summary>
        /// List of keywords in posting e.g. skills, job description, commpany name etc.c 
        /// </summary>
        public string Keywords { get; set; }
        /// <summary>
        /// List of skill to search for 
        /// </summary>
        public List<string> Skill { get; set; }
        /// <summary>
        /// specific country 
        /// </summary>
        public string Country { get; set; }

         
 
   

        #endregion
    }
}
