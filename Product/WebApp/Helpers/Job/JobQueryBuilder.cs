using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddy.Api;
using UpDiddyLib.Dto;

namespace UpDiddy.Helpers.Job
{
    public class JobQueryBuilder
    {
        private IApi _api;
        public string Country { get; set; } = "all";
        public string State { get; set; } = "all";
        public string City { get; set; } = "all";
        private string _industry = "all";
        public string Industry
        {
            get
            {
                return this._industry;
            }
            set
            {
                _industry = value == null ? "all" : value.Replace("-", "+");
            }
        }

        private string _category = "all";
        public string Category
        {
            get
            {
                return this._category;
            }
            set
            {
                _category = value == null ? "all" : value.Replace("-", "+");
            }
        }

        public int Page { get; set; } = 0;

        public JobQueryBuilder(IApi api)
        {
            _api = api;
        }

        public void AddFacet(Facet facet)
        {
            switch(facet.Key)
            {
                case "country":
                    Country = facet.Value;
                    break;
                case "ADMIN_1":
                    State = facet.Value;
                    break;
                case "CITY":
                    City = facet.Value;
                    break;
                case "Industry":
                    Industry = facet.Value;
                    break;
                case "JobCategory":
                    Category = facet.Value;
                    break;
            }
        }

        public async Task<JobSearchResultDto> Execute()
        {
            return await _api.GetJobsUsingRoute(Country, State, City, Industry, Category, Page);
        }
    }
}