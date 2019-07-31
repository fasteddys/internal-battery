using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Interfaces
{
    public interface IHangfireService
    {
        string Enqueue<T>(Expression<Func<T, Task>> methodCall);
        string Enqueue(Expression<Func<Task>> methodCall);
    }
}
