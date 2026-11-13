using System.Linq.Expressions;

namespace KernelR.Domain.Specifications;

/// <summary>
/// Defines a contract for the Specification pattern, which encapsulates a query criterion.
/// </summary>
/// <typeparam name="T">The type of entity the specification applies to.</typeparam>
/// <remarks>
/// <para>
/// A specification is a way to encapsulate a query criterion (a predicate) so that it
/// can be reused, combined, and tested in isolation. This keeps domain logic out of
/// repositories and application services, and makes complex filters composable.
/// </para>
/// <para>
/// <b>Example:</b> Instead of writing <c>o => o.Status == OrderStatus.Active</c> in
/// several places, define an <c>ActiveOrderSpecification</c> once and reuse it everywhere.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class ActiveOrderSpecification : Specification&lt;Order&gt;
/// {
///     public override Expression&lt;Func&lt;Order, bool&gt;&gt; ToExpression()
///         => order => order.Status == OrderStatus.Active;
/// }
/// 
/// // Usage:
/// ISpecification&lt;Order&gt; spec = new ActiveOrderSpecification()
///     .And(new OrderForCustomerSpecification(customerId));
/// var orders = await _repository.ListAsync(spec, cancellationToken);
/// </code>
/// </example>
public interface ISpecification<T>
{
    /// <summary>
    /// Converts the specification to a LINQ expression.
    /// </summary>
    /// <returns>The <see cref="Expression{TDelegate}"/> representing the specification's predicate.</returns>
    Expression<Func<T, bool>> ToExpression();

    /// <summary>
    /// Determines whether the specified entity satisfies the specification.
    /// </summary>
    /// <param name="entity">The entity to evaluate.</param>
    /// <returns><see langword="true"/> if the entity satisfies the specification; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method is intended for in-memory evaluation (e.g., in unit tests).
    /// For database queries, prefer <see cref="ToExpression"/>.
    /// </remarks>
    bool IsSatisfiedBy(T entity);
}
