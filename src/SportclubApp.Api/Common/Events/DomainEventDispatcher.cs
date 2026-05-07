using Microsoft.Extensions.DependencyInjection;

namespace SportclubApp.Api.Common.Events;

public sealed class DomainEventDispatcher(IServiceScopeFactory scopeFactory) : IDomainEventDispatcher
{
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct) where TEvent : class
    {
        using var scope = scopeFactory.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IDomainEventHandler<TEvent>>();
        foreach (var handler in handlers)
        {
            await handler.HandleAsync(@event, ct);
        }
    }
}
