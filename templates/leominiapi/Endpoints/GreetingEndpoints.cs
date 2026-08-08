using LeoMiniApi.Util;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LeoMiniApi.Endpoints;

/// <summary>
///     The endpoints of the greeting resource
/// </summary>
public static class GreetingEndpoints
{
    extension(IEndpointRouteBuilder app)
    {
        public void MapGreetingEndpoints()
        {
            var greetings = app.MapGroup("/hello")
                               .WithTags("Greetings");

            greetings.MapGet("/{name}", GetGreeting)
                     .WithName(nameof(GetGreeting));
        }
    }
    
    private static Results<Ok<GreetingDto>, ValidationProblem> GetGreeting([FromRoute] string name,
                                                                           [FromServices] IClock clock)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return TypedResults.ValidationProblem(Validation.Failure(nameof(name), "Must not be empty"));
        }

        return TypedResults.Ok(new GreetingDto(name, clock.GetCurrentInstant()));
    }
}

public sealed class GreetingDto(string name, Instant timestamp)
{
    public string Message => $"Hello, {name}!";
    public Instant Timestamp => timestamp;
}
