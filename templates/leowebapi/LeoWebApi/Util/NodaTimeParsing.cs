using NodaTime.Text;

namespace LeoWebApi.Util;

/*
 * Minimal APIs bind route, query and header parameters through IParsable<TSelf> or a static TryParse method on the
 * parameter type itself.
 * There is no equivalent of the MVC IModelBinderProvider that could be registered for a type you do not
 * own - see https://github.com/dotnet/aspnetcore/issues/35489.
 *
 * Use these wrappers as the endpoint parameter type; the implicit conversion hands the NodaTime value to the service.
 *
 * Request and response bodies are unaffected.
 */

/// <summary>
///     Binds an ISO formatted <see cref="LocalDate" /> (yyyy-MM-dd) from a route, query or header parameter
/// </summary>
/// <param name="Value">The parsed date</param>
public readonly record struct IsoLocalDate(LocalDate Value) : IParsable<IsoLocalDate>
{
    public static IsoLocalDate Parse(string s, IFormatProvider? provider) => new(LocalDatePattern.Iso.Parse(s).Value);

    public static bool TryParse(string? s, IFormatProvider? provider, out IsoLocalDate result)
    {
        ParseResult<LocalDate> parseResult = LocalDatePattern.Iso.Parse(s ?? string.Empty);

        result = parseResult.Success
            ? new IsoLocalDate(parseResult.Value)
            : default(IsoLocalDate);

        return parseResult.Success;
    }

    public static implicit operator LocalDate(IsoLocalDate isoLocalDate) => isoLocalDate.Value;

    public override string ToString() => LocalDatePattern.Iso.Format(Value);
}

/// <summary>
///     Binds an ISO formatted <see cref="LocalTime" /> (HH:mm:ss) from a route, query or header parameter
/// </summary>
/// <param name="Value">The parsed time</param>
public readonly record struct IsoLocalTime(LocalTime Value) : IParsable<IsoLocalTime>
{
    public static IsoLocalTime Parse(string s, IFormatProvider? provider) =>
        new(LocalTimePattern.ExtendedIso.Parse(s).Value);

    public static bool TryParse(string? s, IFormatProvider? provider, out IsoLocalTime result)
    {
        ParseResult<LocalTime> parseResult = LocalTimePattern.ExtendedIso.Parse(s ?? string.Empty);

        result = parseResult.Success
            ? new IsoLocalTime(parseResult.Value)
            : default(IsoLocalTime);

        return parseResult.Success;
    }

    public static implicit operator LocalTime(IsoLocalTime isoLocalTime) => isoLocalTime.Value;

    public override string ToString() => LocalTimePattern.ExtendedIso.Format(Value);
}

/// <summary>
///     Binds an ISO formatted <see cref="Instant" /> (yyyy-MM-ddTHH:mm:ssZ) from a route, query or header parameter
/// </summary>
/// <param name="Value">The parsed instant</param>
public readonly record struct IsoInstant(Instant Value) : IParsable<IsoInstant>
{
    public static IsoInstant Parse(string s, IFormatProvider? provider) =>
        new(InstantPattern.ExtendedIso.Parse(s).Value);

    public static bool TryParse(string? s, IFormatProvider? provider, out IsoInstant result)
    {
        ParseResult<Instant> parseResult = InstantPattern.ExtendedIso.Parse(s ?? string.Empty);

        result = parseResult.Success
            ? new IsoInstant(parseResult.Value)
            : default(IsoInstant);

        return parseResult.Success;
    }

    public static implicit operator Instant(IsoInstant isoInstant) => isoInstant.Value;

    public override string ToString() => InstantPattern.ExtendedIso.Format(Value);
}
