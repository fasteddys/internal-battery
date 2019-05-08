using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    /// <summary>
    /// This interface, and subsequently its' corresponding concrete class
    /// were created to adhere to the repository pattern we currently have in place.
    /// 
    /// Though it's currently empty, should we want to add functionality down the line,
    /// the interfaces & classes are there to support that, and current code wouldn't
    /// need to be re-factored.
    /// </summary>
    public interface IRecruiterActionRepository : IUpDiddyRepositoryBase<RecruiterAction> { }
}
