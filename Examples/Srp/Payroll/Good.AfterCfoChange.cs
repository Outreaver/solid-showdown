namespace SolidShowdown.Examples.Srp.Payroll.Good.AfterCfoChange;

using SolidShowdown.Examples.Srp.Payroll.Good;

// Same CFO request: "For pay, overtime starts after 7.5 h per day."
// Only Accounting's class changes. HourReporter and EmployeeRepository are not even opened,
// so they cannot break, and HR doesn't need to re-verify anything.
// (Shown as a separate class only so the demo can compare before/after.)

public sealed class PayCalculator
{
    private const decimal RegularHoursPerDay = 7.5m; // ✅ the only edit, in the only class Accounting owns

    public decimal Calculate(EmployeeData e)
    {
        var regular = e.Week.Sum(x => Math.Min(x.Hours, RegularHoursPerDay));
        var overtime = e.TotalHours - regular;
        return regular * e.HourlyRate + overtime * e.HourlyRate * 1.5m;
    }
}
