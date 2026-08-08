using LeoGRpcApi.Api;
using LeoGRpcApi.Shared.ApiContract.Validation;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(o =>
{
    // not using HTTPS, because all production backends _have_ to be behind a reverse proxy which will handle SSL termination
    o.ListenLocalhost(5200, listenOpts =>
                          listenOpts.Protocols = HttpProtocols.Http2);
});

bool isDev = builder.Environment.IsDevelopment();
var configurationManager = builder.Configuration;
builder.Services.LoadAndConfigureSettings(configurationManager);

// we don't have to worry about CORS with gRPC

builder.AddLogging();
builder.Services.AddApplicationServices(configurationManager, isDev);
builder.Services.ConfigureGrpc(isDev);
ValidationSetup.ConfigureValidation();

var app = builder.Build();

app.MapApplicationEndpoints();

await app.RunAsync();