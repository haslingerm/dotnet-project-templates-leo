using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;

namespace LeoGRpcApi.Client.Core.Util;

internal sealed class GrpcClientFactory(IOptions<Settings> options) : IDisposable
{
    private readonly string _host = options.Value.Host;
    private readonly Lock _mutex = new();
    private GrpcChannel? _channel;

    public TClient CreateClient<TClient>() where TClient : ClientBase<TClient>
    {
        lock (_mutex)
        {
            _channel ??= GrpcChannel.ForAddress(_host);
        }
        
        object? obj = Activator.CreateInstance(typeof(TClient), _channel);
        if (obj is not TClient client)
        {
            throw new InvalidOperationException($"Cannot create client of type {typeof(TClient).FullName}");
        }

        return client;
    }

    public void Dispose()
    {
        _channel?.Dispose();
    }
}
