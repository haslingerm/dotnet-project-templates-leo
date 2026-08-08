using System.Diagnostics.CodeAnalysis;
using FluentValidation;

namespace LeoWebApi.Util;

/// <summary>
///     Request validation helpers for the API layer.
///     Validation is always explicit: an endpoint validates its request and decides what to return, nothing runs
///     automatically behind the scenes.
/// </summary>
public static class Validation
{
    /// <summary>
    ///     Validates the given request with the specified validator
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <typeparam name="TValidator">The validator class to use, has to have a parameterless constructor</typeparam>
    /// <typeparam name="TRequest">The request class</typeparam>
    /// <returns>True if the request object passed validation; false otherwise</returns>
    public static bool IsValid<TValidator, TRequest>(TRequest request)
    where TValidator : AbstractValidator<TRequest>, new()
    where TRequest : notnull =>
    IsValid(request, new TValidator(), out _);

    /// <summary>
    ///     Validates the given request with the specified validator and provides the validation errors
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <param name="validationErrors">
    ///     If validation errors occur, they are stored in this out parameter, keyed by property name
    /// </param>
    /// <typeparam name="TValidator">The validator class to use, has to have a parameterless constructor</typeparam>
    /// <typeparam name="TRequest">The request class</typeparam>
    /// <returns>True if the request object passed validation; false otherwise</returns>
    public static bool IsValid<TValidator, TRequest>(TRequest request,
                                                     [NotNullWhen(false)]
                                                     out IDictionary<string, string[]>? validationErrors)
    where TValidator : AbstractValidator<TRequest>, new()
    where TRequest : notnull =>
    IsValid(request, new TValidator(), out validationErrors);

    /// <summary>
    ///     Validates the given request with the provided validator and provides the validation errors.
    ///     Use this overload for validators that need dependencies.
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <param name="validator">The validator instance to use</param>
    /// <param name="validationErrors">
    ///     If validation errors occur, they are stored in this out parameter, keyed by property name
    /// </param>
    /// <typeparam name="TRequest">The request class</typeparam>
    /// <returns>True if the request object passed validation; false otherwise</returns>
    public static bool IsValid<TRequest>(TRequest request, IValidator<TRequest> validator,
                                         [NotNullWhen(false)] out IDictionary<string, string[]>? validationErrors)
    where TRequest : notnull
    {
        var validationResult = validator.Validate(request);

        validationErrors = validationResult.IsValid
        ? null
        : validationResult.ToDictionary();

        return validationResult.IsValid;
    }

    /// <summary>
    ///     Creates a validation error collection for a single property, for checks that are too simple to warrant a
    ///     validator of their own - a route parameter, for example
    /// </summary>
    /// <param name="propertyName">The name of the property the check failed for</param>
    /// <param name="message">The message describing why the check failed</param>
    /// <returns>The validation errors, keyed by property name</returns>
    public static IDictionary<string, string[]> Failure(string propertyName, string message) =>
    new Dictionary<string, string[]>
    {
        [ValidationNaming.ToPropertyName(propertyName)] = [message]
    };
}
