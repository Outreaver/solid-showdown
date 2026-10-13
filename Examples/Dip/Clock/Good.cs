namespace SolidShowdown.Examples.Dip.Clock.Good;

public sealed record User(string Name, DateTimeOffset TrialStartedAt);

// ✅ FOLLOWS DIP: the rule depends on an ABSTRACTION of time, injected from outside.
//    .NET 8+ ships one: System.TimeProvider. In production you pass TimeProvider.System;
//    in tests you pass a fake whose "now" you control.
//
//    Note: TimeProvider is an ABSTRACT CLASS, not an interface. DIP says "depend on
//    abstractions" - an interface is one way to express an abstraction, not the only one.
public sealed class TrialService(TimeProvider clock)
{
    public const int TrialDays = 14;

    public bool IsExpired(User user) =>
        clock.GetUtcNow() > user.TrialStartedAt.AddDays(TrialDays);

    public int DaysLeft(User user) =>
        Math.Max(0, (int)Math.Ceiling((user.TrialStartedAt.AddDays(TrialDays) - clock.GetUtcNow()).TotalDays));
}

// A hand-written fake (the NuGet package Microsoft.Extensions.TimeProvider.Testing
// provides a full-featured FakeTimeProvider if you need timers too).
public sealed class FixedClock(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;
    public override DateTimeOffset GetUtcNow() => Now;
}

// 🤔 CONTEXT: you could also just pass `DateTimeOffset now` as a method parameter:
//    IsExpired(user, now). That's often even simpler - "dependency injection" via arguments.
//    Pick a constructor dependency when many methods need it or when it's buried deep in a call chain.
