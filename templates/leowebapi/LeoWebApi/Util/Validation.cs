using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LeoWebApi.Util;

/// <summary>
///     Request validation helpers for the API layer.
///     Validation is always explicit: an endpoint creates its validator, validates its request and decides what to
///     return, nothing runs automatically behind the scenes.
/// </summary>
public static class Validation
{
    extension<TRequest>(IValidator<TRequest> validator)
        where TRequest : notnull
    {
        /// <summary>
        ///     Validates the given request and provides the result to return when it is invalid.
        ///     The validator is created by the endpoint, so a validator needing extra information simply takes it
        ///     through its constructor.
        /// </summary>
        /// <param name="request">The request to validate</param>
        /// <returns>
        ///     The <see cref="ValidationProblem" /> the endpoint has to return if the request is invalid;
        ///     null if the request passed validation
        /// </returns>
        public ValidationProblem? Check(TRequest request)
        {
            var validationResult = validator.Validate(request);

            return validationResult.IsValid
                ? null
                : TypedResults.ValidationProblem(validationResult.ToDictionary());
        }
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
