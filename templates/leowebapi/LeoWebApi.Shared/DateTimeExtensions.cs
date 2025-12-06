namespace LeoWebApi.Shared;

public static class DateTimeExtensions
{
    extension(IClock clock)
    {
        public ZonedDateTime LocalNow => clock.GetCurrentInstant().ToZonedDateTime();
        public LocalDateTime LocalDateTime => clock.LocalNow.LocalDateTime;
        public LocalTime LocalTime => clock.LocalDateTime.TimeOfDay;
        public LocalDate LocalDate => clock.LocalDateTime.Date;
    }

    extension(Instant instant)
    {
        public ZonedDateTime ToZonedDateTime() => instant.InZone(Const.TimeZone);
    }

    extension(LocalDateTime localDateTime)
    {
        public Instant ToInstantInZone() => localDateTime.Date.ToInstantInZone(localDateTime.TimeOfDay);
    }

    extension(LocalDate localDate)
    {
        public Instant ToInstantInZone(LocalTime? atTime = null)
        {
            var midnight = localDate.AtStartOfDayInZone(Const.TimeZone);
            var effectiveZonedDateTime = atTime.HasValue
                ? midnight.Date.At(atTime.Value).InZoneLeniently(Const.TimeZone)
                : midnight;

            return effectiveZonedDateTime.ToInstant();
        }
    }
}
