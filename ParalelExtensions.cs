using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadImages
{
    public static class Parallel
    {
        public static Task ForEachAsync<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, Task> worker,
            int? maxParallelTaskCount = default,
            CancellationToken cancellationToken = default)
        {
            Debug.Assert(source != null, "source is null");
            Debug.Assert(worker != null, "worker is null");

            return Task.WhenAll(
                Partitioner.Create(source)
                            .GetPartitions(maxParallelTaskCount ?? 100)
                            .Select(partition => Task.Run(
                                async () =>
                                {
                                    using (partition)
                                    {
                                        while (partition.MoveNext())
                                        {
                                            cancellationToken.ThrowIfCancellationRequested();
                                            await worker(partition.Current).ConfigureAwait(false);
                                        }
                                    }
                                })));
        }
    }
}
