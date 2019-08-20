using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Services.JobDataMining
{
    public static class Helpers
    {
        public static string ConvertJValueToString(object obj)
        {
            if (obj != null && obj is JValue casted)
            {
                JValue jValue = (JValue)obj;
                if (jValue != null && jValue.Value != null)
                    return jValue.Value.ToString();
            }
            return null;
        }

        public static async Task ForEachWithDelay<T>(this ICollection<T> items, Func<T, Task> action, double interval)
        {
            using (var timer = new System.Timers.Timer(interval))
            {
                var task = new Task(() => { });
                int remaining = items.Count;
                var queue = new ConcurrentQueue<T>(items);

                timer.Elapsed += async (sender, args) =>
                {
                    T item;
                    if (queue.TryDequeue(out item))
                    {
                        try
                        {
                            await action(item);
                        }
                        finally
                        {
                            // Complete task.
                            remaining -= 1;

                            if (remaining == 0)
                            {
                                // No more items to process. Complete task.
                                task.Start();
                            }
                        }
                    }
                };

                timer.Start();

                await task;
            }
        }

        public class EqualityComparerByUri : IEqualityComparer<JobPage>
        {
            public bool Equals(JobPage x, JobPage y)
            {
                if (Object.ReferenceEquals(x, y))
                    return true;
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;
                return x.Uri == y.Uri;
            }

            public int GetHashCode(JobPage jobPage)
            {
                if (Object.ReferenceEquals(jobPage, null))
                    return 0;
                return jobPage.Uri == null ? 0 : jobPage.Uri.GetHashCode();
            }
        }

        public class CompareByUri : IComparer<JobPage>
        {
            public int Compare(JobPage x, JobPage y)
            {
                return string.Compare(x.Uri.ToString(), y.Uri.ToString());
            }
        }

        public class EqualityComparerByUniqueIdentifier : IEqualityComparer<JobPage>
        {
            public bool Equals(JobPage x, JobPage y)
            {
                if (Object.ReferenceEquals(x, y))
                    return true;
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;
                return x.UniqueIdentifier == y.UniqueIdentifier;
            }

            public int GetHashCode(JobPage jobPage)
            {
                if (Object.ReferenceEquals(jobPage, null))
                    return 0;
                return jobPage.UniqueIdentifier == null ? 0 : jobPage.UniqueIdentifier.GetHashCode();
            }
        }

        public class CompareByJobPageId : IComparer<JobPage>
        {
            public int Compare(JobPage x, JobPage y)
            {
                return x.JobPageId.CompareTo(y.JobPageId);
            }
        }
    }
}
