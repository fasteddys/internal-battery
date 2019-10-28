using Microsoft.AspNetCore.Http;
using System.Net;
using UpDiddyLib.Dto;

namespace UpDiddyApi.Helpers.Job
{
    public class JobQueryHelper
    {



        #region CC2020 

 

        static public JobQueryDto CreateSummaryJobQuery(int PageSize, IQueryCollection query)
        {
            JobQueryDto jobQuery = new JobQueryDto();

            PageSize = GetIntQueryParam(query, "limit", PageSize);
            int PageNum = GetIntQueryParam(query, "offset"); 
            jobQuery.IncludeCouseSkillsHistogram = GetIntQueryParam(query, "includeskillshistogram");


            // map parameters that may have been specified via an url component 
            jobQuery.Country = GetQueryParam(query, "country", "all");
            jobQuery.Province = GetQueryParam(query, "province", "all");
            jobQuery.City = GetQueryParam(query, "city", "all");
            jobQuery.Industry = GetQueryParam(query, "industry", "all");
            jobQuery.JobCategory = GetQueryParam(query, "jobcategory", "all");
            jobQuery.Skill = GetQueryParam(query, "skill", "all");
            // map parameters that can only be specied via a query string parameter 
            jobQuery.Location = GetQueryParam(query, "location");
            jobQuery.PostalCode = GetQueryParam(query, "postalcode");
            jobQuery.StreetAddress = GetQueryParam(query, "streetaddress");
            jobQuery.Keywords = GetQueryParam(query, "keyword");
            jobQuery.DatePublished = GetQueryParam(query, "datepublished");
            jobQuery.CompanyName = GetQueryParam(query, "companyname");
            jobQuery.EmploymentType = GetQueryParam(query, "employmenttype");
            jobQuery.ExperienceLevel = GetQueryParam(query, "experiencelevel");
            jobQuery.EducationLevel = GetQueryParam(query, "educationlevel");
            jobQuery.SearchRadius = GetIntQueryParam(query, "searchradius");

            // Search options
            jobQuery.ExcludeCustomProperties = GetIntQueryParam(query, "excludecustomproperties");
            jobQuery.ExcludeFacets = GetIntQueryParam(query, "excludefacets");
            if (string.IsNullOrWhiteSpace(jobQuery.Country)
                && string.IsNullOrWhiteSpace(jobQuery.Province)
                && string.IsNullOrWhiteSpace(jobQuery.City)
                && string.IsNullOrWhiteSpace(jobQuery.Industry)
                && string.IsNullOrWhiteSpace(jobQuery.JobCategory)
                && string.IsNullOrWhiteSpace(jobQuery.Skill)
                && string.IsNullOrWhiteSpace(jobQuery.Location)
                && string.IsNullOrWhiteSpace(jobQuery.PostalCode)
                && string.IsNullOrWhiteSpace(jobQuery.StreetAddress)
                && string.IsNullOrWhiteSpace(jobQuery.Keywords)
                && string.IsNullOrWhiteSpace(jobQuery.DatePublished)
                && string.IsNullOrWhiteSpace(jobQuery.CompanyName)
                && string.IsNullOrWhiteSpace(jobQuery.EmploymentType)
                && string.IsNullOrWhiteSpace(jobQuery.ExperienceLevel)
                && string.IsNullOrWhiteSpace(jobQuery.EducationLevel)
                && jobQuery.SearchRadius == 0)
            {
                // override the sort order when no seach parameters have been defined (see work item 990 for the reasoning behind this decision)
                jobQuery.OrderBy = "postingPublishTime desc";
            }
            else
            {
                string sort = GetQueryParam(query, "sort");
                string order = GetQueryParam(query, "order");
                if (string.IsNullOrEmpty(sort) == false && string.IsNullOrEmpty(order) == false)
                    jobQuery.OrderBy = sort + " " + order;
            }

            // Commute search
            jobQuery.Lat = GetDoubleQueryParam(query, "lat");
            jobQuery.Lng = GetDoubleQueryParam(query, "lng");
            jobQuery.CommuteTime = GetIntQueryParam(query, "commutetime");

            jobQuery.PreciseAddress = GetBoolQueryParam(query, "preciseaddress");
            jobQuery.PublicTransit = GetBoolQueryParam(query, "publictransit");
            jobQuery.RushHour = GetBoolQueryParam(query, "rushhour");

            // Set up pagination
            jobQuery.PageNum = GetIntQueryParam(query, "pagenum", PageNum);
            jobQuery.PageSize = GetIntQueryParam(query, "pagesize", PageSize);

            return jobQuery;
        }






        #endregion







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
            jobQuery.CompanyName = GetQueryParam(query, "companyname");
            jobQuery.EmploymentType = GetQueryParam(query, "employmenttype");
            jobQuery.ExperienceLevel = GetQueryParam(query, "experience-level");
            jobQuery.EducationLevel = GetQueryParam(query, "education-level");
            jobQuery.SearchRadius = GetIntQueryParam(query, "search-radius");

            // Search options
            jobQuery.ExcludeCustomProperties = GetIntQueryParam(query, "exclude-custom-properties");
            jobQuery.ExcludeFacets = GetIntQueryParam(query, "exclude-facets");
            if (string.IsNullOrWhiteSpace(jobQuery.Country)
                && string.IsNullOrWhiteSpace(jobQuery.Province)
                && string.IsNullOrWhiteSpace(jobQuery.City)
                && string.IsNullOrWhiteSpace(jobQuery.Industry)
                && string.IsNullOrWhiteSpace(jobQuery.JobCategory)
                && string.IsNullOrWhiteSpace(jobQuery.Skill)
                && string.IsNullOrWhiteSpace(jobQuery.Location)
                && string.IsNullOrWhiteSpace(jobQuery.PostalCode)
                && string.IsNullOrWhiteSpace(jobQuery.StreetAddress)
                && string.IsNullOrWhiteSpace(jobQuery.Keywords)
                && string.IsNullOrWhiteSpace(jobQuery.DatePublished)
                && string.IsNullOrWhiteSpace(jobQuery.CompanyName)
                && string.IsNullOrWhiteSpace(jobQuery.EmploymentType)
                && string.IsNullOrWhiteSpace(jobQuery.ExperienceLevel)
                && string.IsNullOrWhiteSpace(jobQuery.EducationLevel)
                && jobQuery.SearchRadius == 0)
            {
                // override the sort order when no seach parameters have been defined (see work item 990 for the reasoning behind this decision)
                jobQuery.OrderBy = "postingPublishTime desc";
            }
            else
            {
                jobQuery.OrderBy = GetQueryParam(query, "order-by");
            }

            // Commute search
            jobQuery.Lat = GetDoubleQueryParam(query, "lat");
            jobQuery.Lng = GetDoubleQueryParam(query, "lng");
            jobQuery.CommuteTime = GetIntQueryParam(query, "commute-time");

            jobQuery.PreciseAddress = GetBoolQueryParam(query, "precise-address");
            jobQuery.PublicTransit = GetBoolQueryParam(query, "public-transit");
            jobQuery.RushHour = GetBoolQueryParam(query, "rush-hour");

            // Set up pagination
            jobQuery.PageNum = GetIntQueryParam(query, "page-num", PageNum);
            jobQuery.PageSize = GetIntQueryParam(query, "page-size", PageSize);

            return jobQuery;
        }

        static public JobQueryDto CreateJobQueryForSimilarJobs(string Province, string City, string Title, int NumSimilarJobs)
        {


            JobQueryDto jobQuery = new JobQueryDto();

            // map parameters that may have been specified via an url component 
            jobQuery.Country = string.Empty;
            jobQuery.Province = Province;
            jobQuery.City = City;
            jobQuery.Industry = string.Empty;
            jobQuery.JobCategory = string.Empty;
            jobQuery.Skill = string.Empty;
            // map parameters that can only be specied via a query string parameter 
            jobQuery.Location = string.Empty;
            jobQuery.PostalCode = string.Empty;
            jobQuery.StreetAddress = string.Empty;
            jobQuery.Keywords = Title;
            jobQuery.DatePublished = string.Empty;
            jobQuery.CompanyName = string.Empty;
            jobQuery.EmploymentType = string.Empty;
            jobQuery.ExperienceLevel = string.Empty;
            jobQuery.EducationLevel = string.Empty;
            jobQuery.SearchRadius = 0;

            // Search options
            jobQuery.ExcludeCustomProperties = 0;
            jobQuery.ExcludeFacets = 0;
            jobQuery.OrderBy = string.Empty;

            // Commute search
            jobQuery.Lat = 0;
            jobQuery.Lng = 0;
            jobQuery.CommuteTime = 0;

            jobQuery.PreciseAddress = false;
            jobQuery.PublicTransit = false;
            jobQuery.RushHour = false;

            // Set up pagination
            jobQuery.PageNum = 0;
            jobQuery.PageSize = NumSimilarJobs;

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
