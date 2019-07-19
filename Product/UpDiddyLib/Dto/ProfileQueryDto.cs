using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class ProfileQueryDto
    {
        #region Pagination 

        /// <summary>
        /// number of jobs to return - do not exceed 100 or talent cloud will throw an exception 
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
        /// order by clause for profile search 
        /// </summary>
        //  "relevance desc": By descending relevance, as determined by the API algorithms.
        //  "update_time desc": Sort by Profile.update_time in descending order(recently updated profiles first).
        //  "create_time desc": Sort by Profile.create_time in descending order(recently created profiles first).
        //  "first_name": Sort by PersonName.PersonStructuredName.given_name in ascending order.
        //  "first_name desc": Sort by PersonName.PersonStructuredName.given_name in descending order.
        //  "last_name": Sort by PersonName.PersonStructuredName.family_name in ascending order.
        //  "last_name desc": Sort by PersonName.PersonStructuredName.family_name in ascending order.  /// </summary>
        public string OrderBy { get; set; }


        #endregion

        #region location data 


        public string Country { get; set; }
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
        public List<string> Skills { get; set; }
        /// <summary>
        /// specific country 
        /// </summary>



        public string EmailAddress { get; set; }

        public string SourcePartner { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Employer { get; set; }



        #endregion
    }
}
