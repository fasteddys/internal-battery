using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IRepositoryWrapper
    {
        ICountryRepository Country { get; }
        IStateRepository State { get; }
        IRecruiterActionRepository RecruiterActionRepository { get; }
    }
}
