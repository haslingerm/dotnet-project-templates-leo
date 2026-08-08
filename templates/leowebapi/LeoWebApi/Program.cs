using LeoWebApi;
using LeoWebApi.Shared;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

bool isDev = builder.Environment.IsDevelopment();
var configurationManager = builder.Configuration;
var settings = builder.Services.LoadAndConfigureSettings(configurationManager);

builder.AddLogging();
builder.Services.AddApplicationServices(configurationManager, isDev);
builder.Services.AddOpenApi();
builder.Services.AddCors(settings);
builder.Services.ConfigureProblemDetails();
builder.Services.ConfigureHttpJsonOptions(o => ConfigureJsonSerialization(o, isDev));
builder.Services.ConfigureAdditionalRouteConstraints();
Setup.ConfigureValidation();

var app = builder.Build();

// not using HTTPS, because all production backends _have_ to be behind a reverse proxy which will handle SSL termination

app.UseCors(Setup.CorsPolicyName);
app.UseExceptionHandler();
app.MapApplicationEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await app.RunAsync();

return;

static void ConfigureJsonSerialization(JsonOptions options, bool isDev)
{
    JsonConfig.ConfigureJsonSerialization(options.SerializerOptions, isDev);
}
