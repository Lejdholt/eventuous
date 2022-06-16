namespace Eventuous;

public class ThrowingApplicationService<T, TState, TId> : IApplicationService<T, TState, TId>, IApplicationService<T>
    where T : Aggregate<TState, TId>
    where TState : AggregateState<TState, TId>, new()
    where TId : AggregateId {
    readonly IApplicationService<T, TState, TId> _inner;

    public ThrowingApplicationService(IApplicationService<T, TState, TId> inner) => _inner = inner;

    public async Task<Result<TState, TId>> Handle(object command,
        Metadata                                         metadata,
        CancellationToken                                cancellationToken) {
        var result = await _inner.Handle(command, metadata, cancellationToken);

        if (result is ErrorResult<TState, TId> error)
            throw error.Exception ?? new ApplicationException($"Error handling command {command}");

        return result;
    }

    async Task<Result> IApplicationService.Handle(object command,
        Metadata                                         metadata,
        CancellationToken                                cancellationToken) {
        var result = await Handle(command, metadata, cancellationToken).NoContext();

        return result switch {
            OkResult<TState, TId>(var aggregateState, var enumerable, _) => new OkResult(aggregateState, enumerable),
            ErrorResult<TState, TId> error => throw error.Exception
                                                 ?? new ApplicationException($"Error handling command {command}"),
            _ => throw new ApplicationException("Unknown result type")
        };
    }
}
