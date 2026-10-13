namespace SolidShowdown.Examples.Srp.Payroll.Bad.AfterCfoChange;

// Six months later the CFO asks: "For PAY purposes, overtime starts after 7.5 h per day."
// A developer opens Employee, finds RegularHours(), sees it feeds CalculatePay(), changes 8 -> 7.5,
// runs Accounting's tests (green!) and ships.
//
// Nobody told HR. Nobody on HR's side asked for anything. Their compliance report now changed.
// (The whole class is copied here only so the demo can run "before" and "after" side by side.)

public sealed record TimeEntry(DayOfWeek Day, decimal Hours);

public class Employee(string name, decimal hourlyRate, IReadOnlyList<TimeEntry> week)
{
    public string Name { get; } = name;
    public decimal HourlyRate { get; } = hourlyRate;
    public IReadOnlyList<TimeEntry> Week { get; } = week;

    public decimal TotalHours => Week.Sum(e => e.Hours);

    public decimal CalculatePay()
    {
        var regular = RegularHours();
        var overtime = TotalHours - regular;
        return regular * HourlyRate + overtime * HourlyRate * 1.5m;
    }

    public string ReportHours() =>
        $"{Name}: {RegularHours():0.##} regular h, {TotalHours - RegularHours():0.##} overtime h";

    public string Save() =>
        $"INSERT INTO employees (name, rate) VALUES (@name, @rate) -- @name = {Name}, @rate = {HourlyRate}";

    // ❌ The only edited line. It was meant for Accounting, but HR's report reads it too.
    private decimal RegularHours() => Week.Sum(e => Math.Min(e.Hours, 7.5m));
}
