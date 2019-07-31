using Hangfire;
using Hangfire.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace UpDiddyApi.Workflow.Helpers
{
    public static class BackgroundJobWrapper
    {
        public static string Enqueue<T>(Expression<Func<T, Task>> methodCall, bool IsPreliminary)
        {
            if (IsPreliminary)
                return string.Empty;

            return BackgroundJob.Enqueue<T>(methodCall);
        }

        public static string Enqueue(Expression<Func<Task>> methodCall, bool IsPreliminary)
        {
            if (IsPreliminary)
                return string.Empty;

            return BackgroundJob.Enqueue(methodCall);
        }
    }
}
