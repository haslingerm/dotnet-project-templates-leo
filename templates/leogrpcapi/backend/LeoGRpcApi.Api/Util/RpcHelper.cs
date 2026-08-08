using FluentValidation;
using Grpc.Core;
using LeoGRpcApi.Shared.ApiContract.Validation;

namespace LeoGRpcApi.Api.Util;

/// <summary>
///     Shortcuts for the statuses an endpoint sets deliberately.
/// </summary>
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
        where TRequest : notnull =>
        RequestValidation.ThrowIfInvalid<TRequest, TValidator>(request);

    /// <summary>
    ///     Throws a <see cref="RpcException" /> with <see cref="StatusCode.InvalidArgument" /> if the request is invalid
    ///     according to the specified validator
    /// </summary>
    /// <param name="request">Request to validate</param>
    /// <param name="validator">Validator to use</param>
    /// <typeparam name="TRequest">Type of the request</typeparam>
    public static void ThrowIfInvalid<TRequest>(TRequest request, IValidator<TRequest> validator)
        where TRequest : notnull =>
        RequestValidation.ThrowIfInvalid(request, validator);

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
