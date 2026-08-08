using FluentValidation;
using FluentValidation.Results;
using Grpc.Core;

namespace LeoGRpcApi.Shared.ApiContract.Validation;

/// <summary>
///     Request validation shared by client and server
/// </summary>
public static class RequestValidation
{
    /// <summary>
    ///     Throws a <see cref="RpcException" /> with <see cref="StatusCode.InvalidArgument" /> if the request is
    ///     invalid according to the specified validator
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <typeparam name="TRequest">Type of the request</typeparam>
    /// <typeparam name="TValidator">Type of the validator, has to have a parameterless constructor</typeparam>
    public static void ThrowIfInvalid<TRequest, TValidator>(TRequest request)
        where TValidator : AbstractValidator<TRequest>, new()
        where TRequest : notnull =>
        ThrowIfInvalid(request, new TValidator());

    /// <summary>
    ///     Throws a <see cref="RpcException" /> with <see cref="StatusCode.InvalidArgument" /> if the request is
    ///     invalid according to the provided validator.
    ///     Use this overload for validators that need dependencies.
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <param name="validator">The validator instance to use</param>
    /// <typeparam name="TRequest">Type of the request</typeparam>
    public static void ThrowIfInvalid<TRequest>(TRequest request, IValidator<TRequest> validator)
        where TRequest : notnull
    {
        var result = validator.Validate(request);

        if (result.IsValid)
        {
            return;
        }

        throw new RpcException(new Status(StatusCode.InvalidArgument, FormatErrors(result.Errors)));
    }

    /// <summary>
    ///     Formats the failures as <c>field: message</c> pairs, so that the caller can tell which field of the
    ///     request was rejected instead of only getting a wall of text
    /// </summary>
    /// <param name="errors">The validation failures to format</param>
    /// <returns>The formatted failures, to be used as the detail of an <see cref="StatusCode.InvalidArgument" /> status</returns>
    private static string FormatErrors(IEnumerable<ValidationFailure> errors) =>
        string.Join("; ", errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
}
