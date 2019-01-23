using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class CountryFactory
    {
        public static Country GetCountryByCountryCode(UpDiddyDbContext db, string countryCode)
        {
            return db.Country
                .Where(s => s.IsDeleted == 0 && s.Code2 == countryCode.Trim())
                .FirstOrDefault();
        }
    }
}
