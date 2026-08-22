using FluentValidation;
using LeoWebApi.Persistence.Model;
using LeoWebApi.Persistence.Util;
using LeoWebApi.Services;
using LeoWebApi.Util;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using ServiceNotFound = OneOf.Types.NotFound;
using ServiceSuccess = OneOf.Types.Success;

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

    private static async ValueTask<Results<Ok<RocketDto>, ValidationProblem, NotFound>>
        GetRocketByIdAsync([FromRoute] int id, [FromServices] IRocketService rocketService)
    {
        if (id < 0)
        {
            return TypedResults.ValidationProblem(Validation.Failure(nameof(id), "Must not be negative"));
        }

        OneOf<Rocket, ServiceNotFound> rocketResult = await rocketService.GetRocketByIdAsync(id, false);

        return rocketResult
            .Match<
                Results<Ok<RocketDto>, ValidationProblem,
                    NotFound>>(rocket => TypedResults.Ok(RocketDto.FromRocket(rocket)),
                               notFound => TypedResults.NotFound());
    }

    private static async ValueTask<Ok<IEnumerable<RocketDto>>> GetAllRocketsAsync(
        [FromServices] IRocketService rocketService)
    {
        IReadOnlyCollection<Rocket> rockets = await rocketService.GetAllRocketsAsync();

        return TypedResults.Ok(rockets.Select(RocketDto.FromRocket));
    }

    private static async ValueTask<Results<CreatedAtRoute<RocketDto>, ValidationProblem>>
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

    private static async ValueTask<Results<NoContent, NotFound>> DeleteRocketAsync(
        [FromRoute] int id, [FromServices] IRocketService rocketService,
        [FromServices] ITransactionProvider transaction)
    {
        await transaction.BeginTransactionAsync();

        OneOf<ServiceSuccess, ServiceNotFound> deleteResult = await rocketService.DeleteRocketAsync(id);

        // the branches are local functions rather than lambdas, so that Match stays readable
        return await deleteResult.Match(OnSuccessAsync, OnNotFoundAsync);

        async ValueTask<Results<NoContent, NotFound>> OnSuccessAsync(ServiceSuccess success)
        {
            await transaction.CommitAsync();

            return TypedResults.NoContent();
        }

        // left uncommitted on purpose, see AddRocketAsync
        static ValueTask<Results<NoContent, NotFound>> OnNotFoundAsync(ServiceNotFound notFound) =>
            ValueTask.FromResult<Results<NoContent, NotFound>>(TypedResults.NotFound());
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
