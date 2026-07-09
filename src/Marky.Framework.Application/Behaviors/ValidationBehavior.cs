using ErrorOr;
using FluentValidation;
using MediatR;

namespace Marky.Framework.Application.Behaviors;

/// <summary>
/// A middleware that intercepts MediatR requests to perform automated validation.
/// </summary>
/// <remarks>
/// Acts as a pre-processor in the request pipeline. If any <see cref="IValidator"/>
/// identifies failures, the pipeline short-circuits, skipping the handler and returning
/// a collection of <see cref="Error.Validation"/> results.
/// </remarks>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Evaluates all registered FluentValidation rules for the current request.
    /// </summary>
    /// <returns>
    /// The next result in the pipeline if validation passes;
    /// otherwise, a <typeparamref name="TResponse"/> (dynamic ErrorOr) containing the validation failures.
    /// </returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct
    )
    {
        if (!validators.Any())
        {
            return await next(ct);
        }

        var context = new ValidationContext<TRequest>(request);
        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            // Converts FluentValidation failures into standardized ErrorOr Validation errors.
            return (dynamic)
                failures
                    .Select(f => Error.Validation(code: f.ErrorCode, description: f.ErrorMessage))
                    .ToList();
        }

        return await next(ct);
    }
}
