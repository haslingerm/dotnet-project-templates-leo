namespace LeoWebApi.Shared;

public static class DateTimeExtensions
{
    public static ZonedDateTime ToZonedDateTime(this Instant self) => self.InZone(Const.TimeZone);
    public static ZonedDateTime GetLocalNow(this IClock self) => self.GetCurrentInstant().ToZonedDateTime();
    public static LocalDateTime GetLocalDateTime(this IClock self) => self.GetLocalNow().LocalDateTime;
    public static LocalTime GetLocalTime(this IClock self) => self.GetLocalDateTime().TimeOfDay;
    public static LocalDate GetLocalDate(this IClock self) => self.GetLocalDateTime().Date;
    public static Instant ToInstantInZone(this LocalDateTime self) => self.Date.ToInstantInZone(self.TimeOfDay);
    public static Instant ToInstantInZone(this LocalDate self, LocalTime? atTime = null)
    {
        var midnight = self.AtStartOfDayInZone(Const.TimeZone);
        var effectiveZonedDateTime = atTime.HasValue
            ? midnight.Date.At(atTime.Value).InZoneLeniently(Const.TimeZone)
            : midnight;

        return effectiveZonedDateTime.ToInstant();
    }
}
