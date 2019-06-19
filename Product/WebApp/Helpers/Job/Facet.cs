using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddy.Helpers.Job
{
    public class Facet
    {
        public string Key { get; set; }
        private string _value { get; set; }
        public string Value
        {
            get
            { 
                return _value != null ? func(_value) : null;
            }
            set
            {
                _value = value;
            }
        }
        /*
         * Used for standardizing the Facet value.
         */
        public Func<string, string> func { get; set; } = (x) => x;

        public static Facet StateFacet(string state = null)
        {
            return new Facet() { Key = "ADMIN_1", Value = state, func = x => x.ToLower() };
        }

        public static Facet CityFacet(string city = null)
        {
            return new Facet() { Key = "CITY", Value = city, func = x => x.Split(',')[0].Replace(' ', '-').Replace('\'', '-').Replace('.', '-') };
        }

        public static Facet IndustryFacet(string industry = null)
        {
            return new Facet() { Key = "Industry", Value = industry, func = x => x.Replace(' ', '-') };
        }

        public static Facet CategoryFacet(string category = null)
        {
            return new Facet() { Key = "JobCategory", Value = category, func = x => x.Replace(' ', '+') };
        }
    }
}