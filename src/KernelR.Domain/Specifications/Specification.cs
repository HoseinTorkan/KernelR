using System.Linq.Expressions;

namespace KernelR.Domain.Specifications;

/// <summary>
/// A base class for implementing the Specification pattern.
/// </summary>
/// <typeparam name="T">The type of entity the specification applies to.</typeparam>
/// <remarks>
/// <para>
/// Subclasses override <see cref="ToExpression"/> to provide the actual predicate. The
/// base class provides the <see cref="And"/>, <see cref="Or"/>, and <see cref="Not"/>
/// combinators, which combine specifications into new specifications.
/// </para>
/// <para>
/// <b>EF Core compatibility:</b> The combinators use a <see cref="ParameterReplacer"/>
/// to rewrite the parameter of each expression, ensuring that the combined expression
/// uses a single shared parameter. Without this, EF Core would fail to translate the
/// resulting expression to SQL (it would see <c>.Invoke()</c> calls it cannot translate).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class ActiveOrderSpecification : Specification&lt;Order&gt;
/// {
///     public override Expression&lt;Func&lt;Order, bool&gt;&gt; ToExpression()
///         => order => order.Status == OrderStatus.Active;
/// }
/// </code>
/// </example>
public abstract class Specification<T> : ISpecification<T>
{
    /// <inheritdoc />
    public abstract Expression<Func<T, bool>> ToExpression();

    /// <inheritdoc />
    public bool IsSatisfiedBy(T entity) => ToExpression().Compile()(entity);

    /// <summary>
    /// Combines the current specification with another specification using a logical AND.
    /// </summary>
    /// <param name="other">The other specification.</param>
    /// <returns>A new specification that is satisfied only when both specifications are satisfied.</returns>
    public Specification<T> And(Specification<T> other)
    {
        return new AndSpecification<T>(this, other);
    }

    /// <summary>
    /// Combines the current specification with another specification using a logical OR.
    /// </summary>
    /// <param name="other">The other specification.</param>
    /// <returns>A new specification that is satisfied when at least one of the specifications is satisfied.</returns>
    public Specification<T> Or(Specification<T> other)
    {
        return new OrSpecification<T>(this, other);
    }

    /// <summary>
    /// Negates the current specification using a logical NOT.
    /// </summary>
    /// <returns>A new specification that is satisfied only when the current specification is not satisfied.</returns>
    public Specification<T> Not()
    {
        return new NotSpecification<T>(this);
    }

    /// <summary>
    /// Creates a specification that always evaluates to true.
    /// </summary>
    /// <returns>A specification that is satisfied by every entity.</returns>
    public static Specification<T> All() => new AllSpecification<T>();

    /// <summary>
    /// Creates a specification that always evaluates to false.
    /// </summary>
    /// <returns>A specification that is never satisfied.</returns>
    public static Specification<T> None() => new NoneSpecification<T>();

    #region Internal Combinators

    private sealed class AndSpecification<TEntity>(Specification<TEntity> left, Specification<TEntity> right) : Specification<TEntity>
    {
        public override Expression<Func<TEntity, bool>> ToExpression()
        {
            Expression<Func<TEntity, bool>> leftExpr = left.ToExpression();
            Expression<Func<TEntity, bool>> rightExpr = right.ToExpression();

            ParameterExpression param = Expression.Parameter(typeof(TEntity), "x");
            BinaryExpression body = Expression.AndAlso(
                new ParameterReplacer(leftExpr.Parameters[0], param).Visit(leftExpr.Body)!,
                new ParameterReplacer(rightExpr.Parameters[0], param).Visit(rightExpr.Body)!);

            return Expression.Lambda<Func<TEntity, bool>>(body, param);
        }
    }

    private sealed class OrSpecification<TEntity>(Specification<TEntity> left, Specification<TEntity> right) : Specification<TEntity>
    {
        public override Expression<Func<TEntity, bool>> ToExpression()
        {
            Expression<Func<TEntity, bool>> leftExpr = left.ToExpression();
            Expression<Func<TEntity, bool>> rightExpr = right.ToExpression();

            ParameterExpression param = Expression.Parameter(typeof(TEntity), "x");
            BinaryExpression body = Expression.OrElse(
                new ParameterReplacer(leftExpr.Parameters[0], param).Visit(leftExpr.Body)!,
                new ParameterReplacer(rightExpr.Parameters[0], param).Visit(rightExpr.Body)!);

            return Expression.Lambda<Func<TEntity, bool>>(body, param);
        }
    }

    private sealed class NotSpecification<TEntity>(Specification<TEntity> spec) : Specification<TEntity>
    {
        public override Expression<Func<TEntity, bool>> ToExpression()
        {
            Expression<Func<TEntity, bool>> expr = spec.ToExpression();
            UnaryExpression body = Expression.Not(expr.Body);

            return Expression.Lambda<Func<TEntity, bool>>(body, expr.Parameters[0]);
        }
    }

    private sealed class AllSpecification<TEntity> : Specification<TEntity>
    {
        public override Expression<Func<TEntity, bool>> ToExpression() => _ => true;
    }

    private sealed class NoneSpecification<TEntity> : Specification<TEntity>
    {
        public override Expression<Func<TEntity, bool>> ToExpression() => _ => false;
    }

    #endregion

    /// <summary>
    /// Replaces a parameter expression within an expression tree with another parameter expression.
    /// </summary>
    /// <remarks>
    /// This is used by the combinators to unify the parameter of two independent expressions
    /// into a single parameter, which is necessary for EF Core to translate the combined
    /// expression to SQL.
    /// </remarks>
    private sealed class ParameterReplacer(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) =>
            node == from ? to : base.VisitParameter(node);
    }
}
