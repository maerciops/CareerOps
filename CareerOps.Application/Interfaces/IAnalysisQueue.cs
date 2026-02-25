namespace CareerOps.Application.Interfaces;

public interface IAnalysisQueue
{
    ValueTask QueueAnalysisAsync(Guid jobId, CancellationToken cancellationToken = default);
    ValueTask<Guid> DequeueAnalysisAsync(CancellationToken cancellationToken = default);
}
