using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;
using X.PagedList;

namespace UpDiddy.ViewModels
{
    public class JobSearchViewModel : BaseViewModel
    {
        public string RequestId { get; set; }
        public string ClientEventId { get; set; }
        public string Keywords { get; set; }
        public string Location { get; set; }
        public IPagedList<JobViewDto> JobsSearchResult { get; set; }
        public Dictionary<Guid, Guid> FavoritesMap { get; set; } = new Dictionary<Guid,Guid>();
        public List<JobQueryFacetDto> Facets { get; set; }
    }
}