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
        string Enqueue<T>(Expression<Action<T>> methodCall);
        string Enqueue(Expression<Func<Task>> methodCall);
        void AddOrUpdate<T>(string recurringJobId, Expression<Action<T>> methodCall, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default");
        string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay);
        void RemoveIfExists(string recurringJobId);


    }
}
