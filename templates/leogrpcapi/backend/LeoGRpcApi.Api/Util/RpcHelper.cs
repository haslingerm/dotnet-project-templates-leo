using FluentValidation;
using Grpc.Core;

namespace LeoGRpcApi.Api.Util;

internal static class RpcHelper
{
    /// <summary>
    ///     Throws a <see cref="RpcException" /> with <see cref="StatusCode.InvalidArgument" /> if the request is invalid
    ///     according to the specified validator
    /// </summary>
    /// <param name="request">Request to validate</param>
    /// <typeparam name="TRequest">Type of the request</typeparam>
    /// <typeparam name="TValidator">Type of the validator</typeparam>
    public static void ThrowIfInvalid<TRequest, TValidator>(TRequest request)
        where TValidator : AbstractValidator<TRequest>, new()
    {
        var validator = new TValidator();
        ThrowIfInvalid(request, validator);
    }

    /// <summary>
    ///     Throws a <see cref="RpcException" /> with <see cref="StatusCode.InvalidArgument" /> if the request is invalid
    ///     according to the specified validator
    /// </summary>
    /// <param name="request">Request to validate</param>
    /// <param name="validator">Validator to use</param>
    /// <typeparam name="TRequest">Type of the request</typeparam>
    /// <typeparam name="TValidator">Type of the validator</typeparam>
    public static void ThrowIfInvalid<TRequest, TValidator>(TRequest request, TValidator validator)
        where TValidator : AbstractValidator<TRequest>
    {
        var result = validator.Validate(request);

        if (result.IsValid)
        {
            return;
        }

        var errorMessage = string.Join(", ", result.Errors.Select(e => e.ErrorMessage));

        throw new RpcException(new Status(StatusCode.InvalidArgument, errorMessage));
    }

    /// <summary>
    ///     Creates a <see cref="RpcException" /> with <see cref="StatusCode.Internal" /> and the specified message
    /// </summary>
    /// <param name="message">Optional message</param>
    /// <returns>An internal error exception</returns>
    public static RpcException InternalError(string? message = null) =>
        new(new Status(StatusCode.Internal, message ?? "An internal error occurred"));

    /// <summary>
    ///     Creates a <see cref="RpcException" /> with <see cref="StatusCode.NotFound" />
    /// </summary>
    /// <returns>A not found exception</returns>
    public static RpcException NotFound() =>
        new(new Status(StatusCode.NotFound, "The requested resource was not found"));
        
    /// <summary>
    ///     Creates a <see cref="RpcException" /> with <see cref="StatusCode.InvalidArgument" />
    /// </summary>
    /// <returns>An invalid data exception</returns>
    public static RpcException Invalid() => new(new Status(StatusCode.InvalidArgument, "The request is invalid"));
    
    /// <summary>
    ///     Creates a <see cref="RpcException" /> with <see cref="StatusCode.Aborted" />
    /// </summary>
    /// <returns>A conflicting operation exception</returns>
    public static RpcException Conflict() =>
        new(new Status(StatusCode.Aborted, "The request conflicts with the current state of the resource"));
}
