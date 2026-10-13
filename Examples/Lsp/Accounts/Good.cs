namespace SolidShowdown.Examples.Lsp.Accounts.Good;

// ✅ The base type only promises what EVERY account can do.
public abstract class Account(decimal opening)
{
    public decimal Balance { get; protected set; } = opening;
    public void Deposit(decimal amount) => Balance += amount;
}

// ✅ Withdrawing is a separate capability with an EXPLICIT contract, fees included.
//    Precondition:  0 < amount and amount + FeeFor(amount) <= Balance + OverdraftLimit
//    Postcondition: Balance decreased by exactly amount + FeeFor(amount)
public interface IWithdrawable
{
    decimal Balance { get; }
    decimal OverdraftLimit { get; }
    decimal FeeFor(decimal amount);
    void Withdraw(decimal amount);
}

public class CheckingAccount(decimal opening) : Account(opening), IWithdrawable
{
    public virtual decimal OverdraftLimit => 0m;
    public virtual decimal FeeFor(decimal amount) => 0m;

    public void Withdraw(decimal amount)
    {
        var total = amount + FeeFor(amount);
        if (amount <= 0 || total > Balance + OverdraftLimit) throw new ArgumentOutOfRangeException(nameof(amount));
        Balance -= total;
    }
}

// ✅ Charges a fee - but the fee is part of the contract, so callers can account for it.
//    Compared with a plain CheckingAccount it requires MORE (the full balance can no longer be
//    withdrawn). That's still substitutable only because IWithdrawable states its precondition
//    and postcondition in terms of FeeFor(amount), which callers can ask for.
public class PayAsYouGoAccount(decimal opening) : CheckingAccount(opening)
{
    public override decimal FeeFor(decimal amount) => 1.00m;
}

// ✅ Accepts everything a CheckingAccount accepts, and more (a weaker precondition).
//    Careful: a client that assumed "Balance never goes negative" WOULD notice. This is
//    substitutable only because IWithdrawable makes OverdraftLimit part of the contract and
//    never promises Balance >= 0. The contract decides, not the "accepts more" rule alone.
public class OverdraftAccount(decimal opening, decimal limit) : CheckingAccount(opening)
{
    public override decimal OverdraftLimit => limit;
}

// ✅ Not withdrawable, and it doesn't pretend to be.
public class FixedTermDeposit(decimal opening, DateOnly maturity) : Account(opening)
{
    public DateOnly Maturity { get; } = maturity;
}

public static class MonthlyFeeJob
{
    // Asks for exactly the capability it uses. A FixedTermDeposit can't even be passed in.
    public static void ChargeAll(IEnumerable<IWithdrawable> accounts, decimal fee)
    {
        foreach (var a in accounts) a.Withdraw(fee);
    }
}

// Rule of thumb (Design by Contract, Bertrand Meyer / Barbara Liskov):
//   a subtype may REQUIRE LESS (weaker preconditions) and PROMISE MORE (stronger postconditions),
//   never the other way round. And it must keep the base type's invariants.
