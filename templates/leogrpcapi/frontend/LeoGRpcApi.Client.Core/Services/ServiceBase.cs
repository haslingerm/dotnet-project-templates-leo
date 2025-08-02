using FluentValidation;
using Grpc.Core;
using LeoGRpcApi.Client.Core.Util;

namespace LeoGRpcApi.Client.Core.Services;

/// <summary>
///     Base class for gRPC service clients
/// </summary>
/// <param name="clientFactory">Factory for creating gRPC clients</param>
/// <typeparam name="TClient">Type of the primary endpoint client this service will use</typeparam>
internal abstract class ServiceBase<TClient>(GrpcClientFactory clientFactory)
    where TClient : ClientBase<TClient>
{
    /// <summary>
    ///     An instance of the primary gRPC client for this service
    /// </summary>
    protected readonly TClient ApiClient = clientFactory.CreateClient<TClient>();
    
    /// <summary>
    ///     Throws a <see cref="RpcException" /> with <see cref="StatusCode.InvalidArgument" /> if the request is invalid
    ///     according to the specified validator
    /// </summary>
    /// <param name="request">Request to validate</param>
    /// <typeparam name="TRequest">Type of the request</typeparam>
    /// <typeparam name="TValidator">Type of the validator</typeparam>
    protected static void ThrowIfInvalid<TRequest, TValidator>(TRequest request)
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
    protected static void ThrowIfInvalid<TRequest, TValidator>(TRequest request, TValidator validator)
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
}
