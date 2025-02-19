using System.Text.Json;
using System.Text.Json.Serialization;
using LeoWebApi;
using LeoWebApi.Shared;
using LeoWebApi.Util;
using Microsoft.AspNetCore.Mvc;
using NodaTime.Serialization.SystemTextJson;

var builder = WebApplication.CreateBuilder(args);

bool isDev = builder.Environment.IsDevelopment();
var configurationManager = builder.Configuration;
var settings = builder.Services.LoadAndConfigureSettings(configurationManager);

builder.AddLogging();
builder.Services.AddApplicationServices(configurationManager, isDev);
builder.Services.AddOpenApi();
builder.Services.AddCors(settings);
builder.Services.AddControllers(o => { o.ModelBinderProviders.Insert(0, new NodaTimeModelBinderProvider()); })
       .AddJsonOptions(o => ConfigureJsonSerialization(o, isDev));
builder.Services.ConfigureAdditionalRouteConstraints();

var app = builder.Build();

// not using HTTPS, because all production backends _have_ to be behind a reverse proxy which will handle SSL termination

app.UseCors(Setup.CorsPolicyName);

app.MapControllers();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await app.RunAsync();

return;

static void ConfigureJsonSerialization(JsonOptions options, bool isDev)
{
    JsonConfig.ConfigureJsonSerialization(options.JsonSerializerOptions, isDev);
}

// used for integration testing
public partial class Program { }