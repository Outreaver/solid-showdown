namespace SolidShowdown.Examples.Srp.Payroll.Bad;

// ❌ BREAKS SRP: one class answers to THREE different actors.
//
//    CalculatePay()  - rules come from Accounting   (reports to the CFO)
//    ReportHours()   - rules come from HR/Operations (reports to the COO)
//    Save()          - rules come from the DBAs      (report to the CTO)
//
// "Responsibility" in SRP does NOT mean "the class does one thing".
// Robert C. Martin's later wording: "A module should be responsible to one, and only one, actor."
// An actor = a group of people who will ask for changes for the same reason.
// Three actors here -> three reasons to change -> one class that gets edited by everybody.

public sealed record TimeEntry(DayOfWeek Day, decimal Hours);

public class Employee(string name, decimal hourlyRate, IReadOnlyList<TimeEntry> week)
{
    public string Name { get; } = name;
    public decimal HourlyRate { get; } = hourlyRate;
    public IReadOnlyList<TimeEntry> Week { get; } = week;

    public decimal TotalHours => Week.Sum(e => e.Hours);

    // Accounting's rule: overtime is paid at 150%.
    public decimal CalculatePay()
    {
        var regular = RegularHours();
        var overtime = TotalHours - regular;
        return regular * HourlyRate + overtime * HourlyRate * 1.5m;
    }

    // HR's rule: the weekly hours report used for labour-law compliance.
    public string ReportHours() =>
        $"{Name}: {RegularHours():0.##} regular h, {TotalHours - RegularHours():0.##} overtime h";

    // The DBAs' rule: how an employee is persisted (simulated - we just build the parameterised SQL).
    public string Save() =>
        $"INSERT INTO employees (name, rate) VALUES (@name, @rate) -- @name = {Name}, @rate = {HourlyRate}";

    // ❌ The trap. Both CalculatePay() and ReportHours() call this helper because
    //    "regular hours" LOOKED like one concept to the developer who wrote it.
    //    Today the code is identical. But it encodes TWO business rules owned by
    //    TWO different people. This is "accidental duplication" that was removed
    //    by merging - DRY applied to code that only looked the same.
    private decimal RegularHours() => Week.Sum(e => Math.Min(e.Hours, 8m));
}
