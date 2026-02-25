using System.Threading.Channels;
using CareerOps.Application.Interfaces;

namespace CareerOps.Infrastructure.BackgroundJobs;

public class AnalysisQueue : IAnalysisQueue
{
    // A bounded channel prevents memory exhaustion if the queue gets too big
    private readonly Channel<Guid> _queue;

    public AnalysisQueue()
    {
        var options = new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<Guid>(options);
    }

    public async ValueTask QueueAnalysisAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        await _queue.Writer.WriteAsync(jobId, cancellationToken);
    }

    public async ValueTask<Guid> DequeueAnalysisAsync(CancellationToken cancellationToken = default)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}