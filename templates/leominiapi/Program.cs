using LeoMiniApi;
using LeoMiniApi.Util;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

bool isDev = builder.Environment.IsDevelopment();

builder.Services.AddApplicationServices();
builder.Services.AddOpenApi();
builder.Services.ConfigureCors();
builder.Services.ConfigureProblemDetails();
builder.Services.ConfigureHttpJsonOptions(o => ConfigureJsonSerialization(o, isDev));

var app = builder.Build();
app.UseCors(Setup.CorsPolicyName);
app.UseExceptionHandler();
app.MapApplicationEndpoints();

if (isDev)
{
    app.MapOpenApi();
}

await app.RunAsync();

return;

static void ConfigureJsonSerialization(JsonOptions options, bool isDev)
{
    JsonConfig.ConfigureJsonSerialization(options.SerializerOptions, isDev);
}
