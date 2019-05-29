using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddyApi.Helpers.Job
{
    public class JobQueryHelper
    {

        static public JobQueryDto CreateJobQuery(string Country, string Province, string City, string Industry, string JobCategory, string Skill, int PageNum, int PageSize, IQueryCollection query)
        {
     
    
            JobQueryDto jobQuery = new JobQueryDto();

            // map parameters that may have been specified via an url component 
            jobQuery.Country = GetQueryParam(query, "country", Country);
            jobQuery.Province = GetQueryParam(query, "province", Province);
            jobQuery.City = GetQueryParam(query, "city", City);
            jobQuery.Industry = GetQueryParam(query, "industry", Industry);
            jobQuery.JobCategory = GetQueryParam(query, "jobcategory", JobCategory);
            jobQuery.Skill = GetQueryParam(query, "skill", Skill);
            // map parameters that can only be specied via a query string parameter 
            jobQuery.Location = GetQueryParam(query, "location");
            jobQuery.PostalCode = GetQueryParam(query, "postal-code");
            jobQuery.StreetAddress = GetQueryParam(query, "street-address");
            jobQuery.Keywords = GetQueryParam(query, "keywords");
            jobQuery.DatePublished = GetQueryParam(query, "datepublished");
            jobQuery.CompanyName = GetQueryParam(query, "company-name");
            jobQuery.EmploymentType = GetQueryParam(query, "employmenttype");
            jobQuery.ExperienceLevel = GetQueryParam(query, "experience-level");
            jobQuery.EducationLevel = GetQueryParam(query, "education-level");
            jobQuery.SearchRadius = GetIntQueryParam(query, "search-radius");

            // Search options
            jobQuery.ExcludeCustomProperties = GetIntQueryParam(query, "exclude-custom-properties");
            jobQuery.ExcludeFacets = GetIntQueryParam(query, "exclude-facets");
            jobQuery.OrderBy = GetQueryParam(query, "order-by");

            // Commute search
            jobQuery.Lat = GetDoubleQueryParam(query, "lat");
            jobQuery.Lng = GetDoubleQueryParam(query, "lng");
            jobQuery.CommuteTime = GetIntQueryParam(query, "commute-time");
           
            jobQuery.PreciseAddress = GetBoolQueryParam(query, "precise-address");
            jobQuery.PublicTransit = GetBoolQueryParam(query, "public-transit");
            jobQuery.RushHour = GetBoolQueryParam(query, "rush-hour"); 

            // Set up pagination
            jobQuery.PageNum = GetIntQueryParam(query, "page-num",PageNum);            
            jobQuery.PageSize = GetIntQueryParam(query, "page-size", PageSize);
            
            return jobQuery;
        }


        #region Private Helper Functions

        // map search parameters.  Note: Querystring params have higher priority than url path params
        static private string GetQueryParam(IQueryCollection queryInfo, string ParamName, string urlComponentValue = "")
        {
            // first check to see if the param was specified in the query string.  Highest priority 
            if (queryInfo.Keys.Contains(ParamName) && string.IsNullOrEmpty(queryInfo[ParamName]) == false && queryInfo[ParamName] != "all")
                return WebUtility.UrlDecode(queryInfo[ParamName]).Trim();

            // check to see if the param was specified through an url component 
            if (urlComponentValue != null && string.IsNullOrEmpty(urlComponentValue) == false && urlComponentValue != "all")
                return WebUtility.UrlDecode(urlComponentValue).Trim();
            
            // empty string -> not specified 
             return string.Empty;
        }

        static private int GetIntQueryParam(IQueryCollection queryInfo, string ParamName, int urlComponentValue = 0)
        {
            try
            {
                // first check to see if the param was specified in the query string.  Highest priority 
                if (queryInfo.Keys.Contains(ParamName) && string.IsNullOrEmpty(queryInfo[ParamName]) == false && queryInfo[ParamName] != "all")
                    return int.Parse(queryInfo[ParamName]);

                // check to see if the param was specified through an url component 
                if (urlComponentValue != null)
                    return urlComponentValue;

                // zero -> value not entered 
                return 0;
            }
            catch
            {
                return 0;
            }      
        }


        static private double GetDoubleQueryParam(IQueryCollection queryInfo, string ParamName, double urlComponentValue = 0)
        {
            try
            {
                // first check to see if the param was specified in the query string.  Highest priority 
                if (queryInfo.Keys.Contains(ParamName) && string.IsNullOrEmpty(queryInfo[ParamName]) == false && queryInfo[ParamName] != "all")
                    return double.Parse(queryInfo[ParamName]);

                // check to see if the param was specified through an url component 
                if (urlComponentValue != null)
                    return urlComponentValue;

                // zero -> value not entered 
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        static private bool GetBoolQueryParam(IQueryCollection queryInfo, string ParamName, bool urlComponentValue = false)
        {
            try
            {
                // first check to see if the param was specified in the query string.  Highest priority 
                if (queryInfo.Keys.Contains(ParamName) && string.IsNullOrEmpty(queryInfo[ParamName]) == false && queryInfo[ParamName] != "all")
                    return queryInfo[ParamName].ToString().ToLower() == "true" ? true : false;

                // check to see if the param was specified through an url component 
                if (urlComponentValue != null)
                    return urlComponentValue;

                //  assume false 
                return false;
            }
            catch
            {
                return false;
            }
        }




        #endregion


    }
}
