using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IZeroBounceRepository : IUpDiddyRepositoryBase<ZeroBounce>
    {
        /// <summary>
        /// Retrieves the most recent request that we made to ZeroBounce in the last 90 days (if it exists).
        /// </summary>
        /// <param name="email"></param>
        /// <returns>TRUE if the email was deemed valid, FALSE if it was deemed invalid, and NULL if no request was made in the last 90 days (or the result cannot be determined).</returns>
        Task<bool?> MostRecentResultInLast90Days(string email);
    }
}
