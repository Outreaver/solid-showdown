using SolidShowdown.Catalog;
using Before = SolidShowdown.Examples.Srp.Payroll.Bad;
using After = SolidShowdown.Examples.Srp.Payroll.Bad.AfterCfoChange;
using Good = SolidShowdown.Examples.Srp.Payroll.Good;
using GoodAfter = SolidShowdown.Examples.Srp.Payroll.Good.AfterCfoChange;

namespace SolidShowdown.Examples.Srp.Payroll;

public static class Demo
{
    // Mon 9h, Tue 8h, Wed 10h, Thu 8h, Fri 6h  -> 41h total
    private static readonly (DayOfWeek Day, decimal Hours)[] Week =
        [(DayOfWeek.Monday, 9), (DayOfWeek.Tuesday, 8), (DayOfWeek.Wednesday, 10), (DayOfWeek.Thursday, 8), (DayOfWeek.Friday, 6)];

    public static void RunBad(DemoLog log)
    {
        var before = new Before.Employee("Alice", 20m, [.. Week.Select(w => new Before.TimeEntry(w.Day, w.Hours))]);
        var after = new After.Employee("Alice", 20m, [.. Week.Select(w => new After.TimeEntry(w.Day, w.Hours))]);

        log.Header("Before the CFO's change");
        log.Info($"Pay:        {before.CalculatePay():0.00}");
        log.Info($"HR report:  {before.ReportHours()}");

        log.Header("After the CFO's change (8h → 7.5h, edited in Employee.RegularHours)");
        log.Info($"Pay:        {after.CalculatePay():0.00}");
        log.Info($"HR report:  {after.ReportHours()}");

        log.Header("What each actor sees");
        log.Check("CFO: pay reflects the new overtime rule", () => after.CalculatePay() != before.CalculatePay());
        log.Check("COO: HR report unchanged (nobody in HR asked for a change)", () => after.ReportHours() == before.ReportHours());
        log.Warn("HR's compliance report now claims Alice worked 5h overtime instead of 3h.");
        log.Muted("Accounting's tests were green. The bug lands in a different department's numbers.");
    }

    public static void RunGood(DemoLog log)
    {
        var alice = new Good.EmployeeData("Alice", 20m, [.. Week.Select(w => new Good.TimeEntry(w.Day, w.Hours))]);
        var hr = new Good.HourReporter();
        var payBefore = new Good.PayCalculator().Calculate(alice);
        var payAfter = new GoodAfter.PayCalculator().Calculate(alice);

        log.Header("Before the CFO's change");
        log.Info($"Pay:        {payBefore:0.00}");
        log.Info($"HR report:  {hr.Report(alice)}");

        log.Header("After the CFO's change (only PayCalculator edited)");
        log.Info($"Pay:        {payAfter:0.00}");
        log.Info($"HR report:  {hr.Report(alice)}");

        log.Header("What each actor sees");
        log.Check("CFO: pay reflects the new overtime rule", () => payAfter != payBefore);
        log.Check("COO: HR report unchanged", () => hr.Report(alice) == "Alice: 38 regular h, 3 overtime h");
        log.Muted("HourReporter was never opened, so it cannot have been broken.");
    }
}
