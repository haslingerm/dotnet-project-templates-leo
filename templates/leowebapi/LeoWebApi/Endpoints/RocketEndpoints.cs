using System.Diagnostics;
using FluentValidation;
using LeoWebApi.Persistence.Model;
using LeoWebApi.Persistence.Util;
using LeoWebApi.Services;
using LeoWebApi.Util;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using LeoUnion;
// the service markers would clash with ASP.NET Core's typed results (TypedResults.NotFound())
using ServiceNotFound = LeoUnion.Markers.NotFound;
using ServiceSuccess = LeoUnion.Markers.Success;

namespace LeoWebApi.Endpoints;

/// <summary>
///     The endpoints of the rocket resource
/// </summary>
public static class RocketEndpoints
{
    extension(IEndpointRouteBuilder app)
    {
        public void MapRocketEndpoints()
        {
            var rockets = app.MapGroup("/api/rockets")
                             .WithTags("Rockets");

            rockets.MapGet("/{id:int}", GetRocketByIdAsync)
                   .WithName(nameof(GetRocketByIdAsync));
            rockets.MapGet("/", GetAllRocketsAsync);
            rockets.MapPost("/", AddRocketAsync);
            rockets.MapDelete("/{id:int}", DeleteRocketAsync);
        }
    }

    private static async Task<Results<Ok<RocketDto>, ValidationProblem, NotFound>>
        GetRocketByIdAsync([FromRoute] int id, [FromServices] IRocketService rocketService)
    {
        if (id < 0)
        {
            return TypedResults.ValidationProblem(Validation.Failure(nameof(id), "Must not be negative"));
        }

        Union<Rocket, ServiceNotFound> rocketResult = await rocketService.GetRocketByIdAsync(id, false);

        // the compiler checks that every case of the union is handled
        return rocketResult switch
        {
            Rocket rocket => TypedResults.Ok(RocketDto.FromRocket(rocket)),
            ServiceNotFound => TypedResults.NotFound()
        };
    }

    private static async Task<Ok<IEnumerable<RocketDto>>> GetAllRocketsAsync(
        [FromServices] IRocketService rocketService)
    {
        IReadOnlyCollection<Rocket> rockets = await rocketService.GetAllRocketsAsync();

        return TypedResults.Ok(rockets.Select(RocketDto.FromRocket));
    }

    private static async Task<Results<CreatedAtRoute<RocketDto>, ValidationProblem>>
        AddRocketAsync([FromBody] AddRocketRequest addRequest,
                       [FromServices] IRocketService rocketService,
                       [FromServices] ITransactionProvider transaction)
    {
        if (new AddRocketRequest.Validator().Check(addRequest) is { } validationProblem)
        {
            return validationProblem;
        }

        await transaction.BeginTransactionAsync();

        var rocket = await rocketService.AddRocketAsync(addRequest.Manufacturer, addRequest.ModelName,
                                                        addRequest.MaxThrust, addRequest.PayloadDeltaV);

        // committing is explicit in the success case, rolling back is the default
        await transaction.CommitAsync();

        return TypedResults.CreatedAtRoute(RocketDto.FromRocket(rocket), nameof(GetRocketByIdAsync), new
        {
            rocket.Id
        });
    }

    private static async Task<Results<NoContent, NotFound>> DeleteRocketAsync(
        [FromRoute] int id, [FromServices] IRocketService rocketService,
        [FromServices] ITransactionProvider transaction)
    {
        await transaction.BeginTransactionAsync();

        Union<ServiceSuccess, ServiceNotFound> deleteResult = await rocketService.DeleteRocketAsync(id);

        // A switch statement, because the success branch also commits. The compiler does not know that it handles
        // every case, so it needs the default guard; that guard is no case: LA0003 still checks that all are handled.
        switch (deleteResult)
        {
            case ServiceSuccess:
                await transaction.CommitAsync();

                return TypedResults.NoContent();
            case ServiceNotFound:
                // left uncommitted on purpose, see AddRocketAsync
                return TypedResults.NotFound();
            default:
                throw new UnreachableException();
        }
    }
}

public sealed class AddRocketRequest
{
    public required string ModelName { get; init; }
    public required string Manufacturer { get; init; }
    public double MaxThrust { get; init; }
    public long PayloadDeltaV { get; init; }

    public sealed class Validator : AbstractValidator<AddRocketRequest>
    {
        public Validator()
        {
            RuleFor(x => x.ModelName).NotEmpty();
            RuleFor(x => x.Manufacturer).NotEmpty();
            RuleFor(x => x.MaxThrust).GreaterThan(0);
            RuleFor(x => x.PayloadDeltaV).GreaterThan(0);
        }
    }
}

public sealed class RocketDto
{
    public int Id { get; init; }
    public required string ModelName { get; init; }
    public required string Manufacturer { get; init; }
    public double MaxThrust { get; init; }
    public long PayloadDeltaV { get; init; }

    public static RocketDto FromRocket(Rocket rocket) =>
        new()
        {
            Id = rocket.Id,
            ModelName = rocket.ModelName,
            Manufacturer = rocket.Manufacturer,
            MaxThrust = rocket.MaxThrust,
            PayloadDeltaV = rocket.PayloadDeltaV
        };
}
