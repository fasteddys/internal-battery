using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddy.ViewModels
{
    public class CountryViewModel
    {
        public Guid CountryGuid { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Sequence { get; set; }
    }

    public class StateViewModel
    {
        public Guid StateGuid { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Sequence { get; set; }
    }

    public class Thing
    {
        public CountryViewModel SelectedCountry { get; set; }
        public IEnumerable<SelectListItem> AllCountries { get; set; }
        public IEnumerable<SelectListItem> GetStatesByCountry(CountryViewModel countryViewModel)
        {
            throw new NotImplementedException();
        }
        public StateViewModel SelectedState { get; set; }
        public IEnumerable<SelectListItem> AllStates { get; set; }
    }
}