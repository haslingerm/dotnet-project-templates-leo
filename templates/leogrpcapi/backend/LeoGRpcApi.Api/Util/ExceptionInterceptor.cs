using System.Diagnostics;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace LeoGRpcApi.Api.Util;

/// <summary>
///     Turns unhandled exceptions into a gRPC status the caller can act on
/// </summary>
/// <remarks>
///     A <see cref="RpcException" /> is passed through untouched: it is a status an endpoint set on purpose and
///     therefore part of the contract, not a failure.
/// </remarks>
public sealed class ExceptionInterceptor(ILogger<ExceptionInterceptor> logger) : Interceptor
{
    private const string TraceIdTrailer = "trace-id";

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (Exception ex)
        {
            throw ToRpcException(ex, context);
        }
    }

    public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream, ServerCallContext context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(requestStream, context);
        }
        catch (Exception ex)
        {
            throw ToRpcException(ex, context);
        }
    }

    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            await continuation(request, responseStream, context);
        }
        catch (Exception ex)
        {
            throw ToRpcException(ex, context);
        }
    }

    public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context, DuplexStreamingServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            await continuation(requestStream, responseStream, context);
        }
        catch (Exception ex)
        {
            throw ToRpcException(ex, context);
        }
    }

    private RpcException ToRpcException(Exception exception, ServerCallContext context)
    {
        switch (exception)
        {
            case RpcException rpcException:
            {
                logger.LogInformation("The call to {Method} was answered with {StatusCode}: {Detail}",
                                      context.Method, rpcException.StatusCode, rpcException.Status.Detail);

                return rpcException;
            }
            case OperationCanceledException when context.CancellationToken.IsCancellationRequested:
            {
                // the caller gave up on the call, that is not a server side failure
                return new RpcException(new Status(StatusCode.Cancelled, "The call was cancelled"));
            }
        }
        
        string traceId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        logger.LogError(exception, "An unhandled exception occurred while processing the call to {Method} ({TraceId})",
                        context.Method, traceId);

        return new RpcException(new Status(StatusCode.Internal, "An unexpected error occurred"),
                                new Metadata
                                {
                                    { TraceIdTrailer, traceId }
                                });
    }
}
