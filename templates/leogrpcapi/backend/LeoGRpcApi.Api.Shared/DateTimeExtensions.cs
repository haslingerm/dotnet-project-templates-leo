namespace LeoGRpcApi.Api.Shared;

public static class DateTimeExtensions
{
    extension(Instant self)
    {
        public ZonedDateTime ToZonedDateTime() => self.InZone(Const.TimeZone);
    }

    extension(IClock self)
    {
        public ZonedDateTime LocalNow => self.GetCurrentInstant().ToZonedDateTime();
        public LocalDateTime LocalDateTime => self.LocalNow.LocalDateTime;
        public LocalTime LocalTime => self.LocalDateTime.TimeOfDay;
        public LocalDate LocalDate => self.LocalDateTime.Date;
    }

    extension(LocalDateTime self)
    {
        public Instant ToInstantInZone() => self.Date.ToInstantInZone(self.TimeOfDay);
    }

    extension(LocalDate self)
    {
        public Instant ToInstantInZone(LocalTime? atTime = null)
        {
            var midnight = self.AtStartOfDayInZone(Const.TimeZone);
            var effectiveZonedDateTime = atTime.HasValue
                ? midnight.Date.At(atTime.Value).InZoneLeniently(Const.TimeZone)
                : midnight;

            return effectiveZonedDateTime.ToInstant();
        }
    }
}
