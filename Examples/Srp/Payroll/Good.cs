namespace SolidShowdown.Examples.Srp.Payroll.Good;

// ✅ FOLLOWS SRP: separate the code that different actors depend on.
//    Each class below has exactly one group of people who can ask for it to change.

public sealed record TimeEntry(DayOfWeek Day, decimal Hours);

// Plain data. No business rule lives here, so no actor "owns" it.
// (It will still change if the data itself changes - e.g. a new field. See the scenarios.)
public sealed record EmployeeData(string Name, decimal HourlyRate, IReadOnlyList<TimeEntry> Week)
{
    public decimal TotalHours => Week.Sum(e => e.Hours);
}

// ✅ Owned by Accounting (CFO).
public sealed class PayCalculator
{
    private const decimal RegularHoursPerDay = 8m;

    public decimal Calculate(EmployeeData e)
    {
        var regular = RegularHours(e);
        var overtime = e.TotalHours - regular;
        return regular * e.HourlyRate + overtime * e.HourlyRate * 1.5m;
    }

    private static decimal RegularHours(EmployeeData e) =>
        e.Week.Sum(x => Math.Min(x.Hours, RegularHoursPerDay));
}

// ✅ Owned by HR / Operations (COO).
public sealed class HourReporter
{
    // Yes, this is the same formula as in PayCalculator. That is DELIBERATE.
    // It is the same CODE but not the same KNOWLEDGE: "regular hours for pay" and
    // "regular hours for labour-law reporting" are decided by different people and
    // will drift apart. DRY is about knowledge, not about identical lines.
    private const decimal RegularHoursPerDay = 8m;

    public string Report(EmployeeData e)
    {
        var regular = e.Week.Sum(x => Math.Min(x.Hours, RegularHoursPerDay));
        return $"{e.Name}: {regular:0.##} regular h, {e.TotalHours - regular:0.##} overtime h";
    }
}

// ✅ Owned by the DBAs (CTO).
public sealed class EmployeeRepository
{
    public string Save(EmployeeData e) =>
        $"INSERT INTO employees (name, rate) VALUES (@name, @rate) -- @name = {e.Name}, @rate = {e.HourlyRate}";
}

// Optional: callers who liked the old "one object" API can get it back through a facade.
// The facade only delegates, so it has almost no reason to change by itself.
public sealed class EmployeeFacade(EmployeeData data)
{
    private readonly PayCalculator _pay = new();
    private readonly HourReporter _hours = new();
    private readonly EmployeeRepository _repository = new();

    public decimal CalculatePay() => _pay.Calculate(data);
    public string ReportHours() => _hours.Report(data);
    public string Save() => _repository.Save(data);
}

// 🤔 CONTEXT: whether this split is "right" depends on your organisation, not on the code.
//    In a 4-person startup where the founder decides pay, reporting AND the database,
//    there is ONE actor - the original Employee class would not violate SRP there, and
//    four classes would be ceremony. Re-evaluate when the org chart changes:
//    Conway's law and SRP are two sides of the same coin.
