using Result;

namespace OptionType.Sample;

public class Discount
{
    public decimal Percentage { get; }
    public Option<DateTime> StartDate { get; }
    public Option<DateTime> EndDate { get; }
    public Option<DayOfWeek> DayOfWeek { get; }

    private Discount(decimal percentage, Option<DateTime> startDate, Option<DateTime> endDate, Option<DayOfWeek> dayOfWeek)
    {
        Percentage = percentage;
        StartDate = startDate;
        EndDate = endDate;
        DayOfWeek = dayOfWeek;
    }

    public static Result<Discount> Create(decimal percentage, Option<DateTime> startDate, Option<DateTime> endDate) =>
        Create(percentage, startDate, endDate, Option<DayOfWeek>.None());

    public static Result<Discount> Create(decimal percentage, Option<DateTime> startDate, Option<DateTime> endDate, Option<DayOfWeek> dayOfWeek)
    {
        if (percentage < 0 || percentage > 100)
            return Result<Discount>.Error("Discount percentage must be between 0 and 100.");
            
        return Result<Discount>.Success(new Discount(percentage, startDate, endDate, dayOfWeek));
    }

    public bool IsValid()
    {
        var now = DateTime.Now;
        var isValidStartDate = !StartDate.Map(start => start > now).Unwrap(() => false);
        var isValidEndDate = !EndDate.Map(end => end < now).Unwrap(() => false);
        var isValidDayOfWeek = !DayOfWeek.Map(day => day != DateTime.Now.DayOfWeek).Unwrap(() => false);

        return isValidStartDate && isValidEndDate && isValidDayOfWeek;
    }
}
