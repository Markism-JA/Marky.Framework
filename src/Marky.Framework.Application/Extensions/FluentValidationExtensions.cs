using ErrorOr;
using FluentValidation;

namespace Marky.Framework.Application.Extensions;

/// <summary>
/// Provides extension methods for <see cref="FluentValidation"/> to integrate with the <see cref="ErrorOr"/> library.
/// </summary>
public static class FluentValidationExtensions
{
    /// <summary>
    /// Maps a domain-specific <see cref="Error"/> to a FluentValidation rule.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="rule">The FluentValidation rule builder.</param>
    /// <param name="error">The <see cref="Error"/> containing the Code and Description to be used.</param>
    /// <returns>The modified rule builder options for further chaining.</returns>
    /// <remarks>
    /// This method synchronizes validation failures with the application's domain error codes.
    /// Instead of hardcoding strings in validators, it extracts the <see cref="Error.Code"/>
    /// and <see cref="Error.Description"/> from a centralized Error object.
    /// </remarks>
    public static IRuleBuilderOptions<T, TProperty> WithCustomError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        Error error
    )
    {
        return rule.WithErrorCode(error.Code).WithMessage(error.Description);
    }
}
