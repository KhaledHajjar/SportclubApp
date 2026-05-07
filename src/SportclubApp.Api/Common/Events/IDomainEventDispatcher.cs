namespace SportclubApp.Api.Common.Events;

public interface IDomainEventDispatcher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct) where TEvent : class;
}

public interface IDomainEventHandler<in TEvent> where TEvent : class
{
    Task HandleAsync(TEvent @event, CancellationToken ct);
}
