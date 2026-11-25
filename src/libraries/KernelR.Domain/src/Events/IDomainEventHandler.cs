namespace KernelR.Domain.Events;

/// <summary>
/// Defines a handler for a specific type of domain event.
/// </summary>
/// <typeparam name="TEvent">
/// The type of the domain event to handle.
/// </typeparam>
/// <remarks>
/// <para>
/// Domain event handlers are responsible for processing domain events
/// of the specified type. Multiple handlers may be registered for
/// the same event type.
/// </para>
/// <para>
/// The execution order, dispatching strategy, transaction boundaries,
/// and exception handling are determined by the event dispatcher.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class OrderPlacedEventHandler : IDomainEventHandler&lt;OrderPlacedEvent&gt;
/// {
///     public Task HandleAsync(
///         OrderPlacedEvent domainEvent,
///         CancellationToken cancellationToken = default)
///     {
///         // Handle the event.
///         return Task.CompletedTask;
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="IDomainEvent"/>
/// <seealso cref="IDomainEventDispatcher"/>
public interface IDomainEventHandler<TEvent>
    where TEvent : IDomainEvent
{
    /// <summary>
    /// Handles the specified domain event.
    /// </summary>
    /// <param name="domainEvent">
    /// The domain event to handle.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the handling operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous handling operation.
    /// </returns>
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
