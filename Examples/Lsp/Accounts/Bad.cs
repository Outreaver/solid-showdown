namespace SolidShowdown.Examples.Lsp.Accounts.Bad;

// The CONTRACT of Account.Withdraw (what callers are entitled to assume):
//   Precondition:  0 < amount <= Balance
//   Postcondition: Balance decreased by EXACTLY `amount`
public class Account(decimal opening)
{
    public decimal Balance { get; protected set; } = opening;

    public void Deposit(decimal amount) => Balance += amount;

    public virtual void Withdraw(decimal amount)
    {
        if (amount <= 0 || amount > Balance) throw new ArgumentOutOfRangeException(nameof(amount));
        Balance -= amount;
    }
}

public class CheckingAccount(decimal opening) : Account(opening);

// ❌ BREAKS LSP - strengthened precondition.
//    The base type accepts any amount <= Balance; this subtype accepts NOTHING.
//    Code that works with "an Account" blows up when handed this one.
public class FixedTermDeposit(decimal opening) : Account(opening)
{
    public override void Withdraw(decimal amount) =>
        throw new InvalidOperationException("Funds are locked until maturity.");
}

// ❌ BREAKS LSP - weakened postcondition.
//    Silently takes a 1.00 fee, so Balance drops by MORE than `amount`.
//    A transfer between accounts now loses money, and nobody gets an exception.
public class PayAsYouGoAccount(decimal opening) : Account(opening)
{
    public override void Withdraw(decimal amount)
    {
        base.Withdraw(amount);
        Balance -= 1.00m;
    }
}

// A client written against Account. It is not wrong - it trusts the contract.
public static class MonthlyFeeJob
{
    public static void ChargeAll(IEnumerable<Account> accounts, decimal fee)
    {
        foreach (var a in accounts) a.Withdraw(fee); // crashes half-way through on a FixedTermDeposit
    }
}

// 🤔 CONTEXT: whether PayAsYouGoAccount is a violation depends ENTIRELY on the written contract.
//    If Account documented "balance decreases by amount plus any applicable fees", it would be fine.
//    .NET does this on purpose: ICollection<T>.Add may throw NotSupportedException, but only when
//    IsReadOnly is true, and Stream.Write may throw unless CanWrite - the capability query is PART
//    of the contract. That's legal, but it pushes checks onto every client. Make contracts explicit.
