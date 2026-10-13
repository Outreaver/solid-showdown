using SolidShowdown.Catalog;
using Bad = SolidShowdown.Examples.Dip.Clock.Bad;
using Good = SolidShowdown.Examples.Dip.Clock.Good;

namespace SolidShowdown.Examples.Dip.Clock;

public static class Demo
{
    // A test someone wrote on 2026-01-10, with a fixed trial start date (5 days earlier).
    private static readonly DateTimeOffset TrialStart = new(2026, 1, 5, 9, 0, 0, TimeSpan.Zero);

    public static void RunBad(DemoLog log)
    {
        var service = new Bad.TrialService();
        var user = new Bad.User("Ann", TrialStart);

        log.Header($"Tests written on 2026-01-10 - running now, on {DateTimeOffset.UtcNow:yyyy-MM-dd}");
        log.Check("5 days into the trial: not expired", () => !service.IsExpired(user));
        log.Check("5 days into the trial: 9 days left", () => service.DaysLeft(user) == 9);
        log.Warn("These tests passed on the day they were written. Nothing in the code changed.");
        log.Warn("To test 'expires after day 14' you'd have to wait two weeks - or edit TrialService.");
    }

    public static void RunGood(DemoLog log)
    {
        var clock = new Good.FixedClock(TrialStart.AddDays(5));
        var service = new Good.TrialService(clock);
        var user = new Good.User("Ann", TrialStart);

        log.Header("Same tests, the clock is injected (fixed at 2026-01-10)");
        log.Check("5 days into the trial: not expired", () => !service.IsExpired(user));
        log.Check("5 days into the trial: 9 days left", () => service.DaysLeft(user) == 9);

        log.Header("Time travel is now one line");
        clock.Now = TrialStart.AddDays(14);
        log.Check("exactly at day 14: not yet expired", () => !service.IsExpired(user));
        clock.Now = TrialStart.AddDays(14).AddSeconds(1);
        log.Check("one second after day 14: expired", () => service.IsExpired(user));

        log.Header("Production");
        var real = new Good.TrialService(TimeProvider.System);
        log.Info($"With TimeProvider.System, Ann's trial expired: {real.IsExpired(user)}");
    }
}
