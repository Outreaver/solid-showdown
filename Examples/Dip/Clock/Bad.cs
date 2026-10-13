namespace SolidShowdown.Examples.Dip.Clock.Bad;

public sealed record User(string Name, DateTimeOffset TrialStartedAt);

// ❌ BREAKS DIP: a business rule reaches straight for a global, volatile detail - the
//    system clock. `DateTimeOffset.UtcNow` is a hidden dependency: it isn't in the constructor,
//    it isn't in the method signature, and you can't replace it.
//    Results depend on WHEN the code runs. Tests that pass today fail next month.
public sealed class TrialService
{
    public const int TrialDays = 14;

    public bool IsExpired(User user) =>
        DateTimeOffset.UtcNow > user.TrialStartedAt.AddDays(TrialDays); // ❌ hidden dependency

    public int DaysLeft(User user) =>
        Math.Max(0, (int)Math.Ceiling((user.TrialStartedAt.AddDays(TrialDays) - DateTimeOffset.UtcNow).TotalDays));
}
