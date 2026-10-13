using SolidShowdown.Catalog;
using Bad = SolidShowdown.Examples.Lsp.Accounts.Bad;
using Good = SolidShowdown.Examples.Lsp.Accounts.Good;

namespace SolidShowdown.Examples.Lsp.Accounts;

public static class Demo
{
    public static void RunBad(DemoLog log)
    {
        (string, Func<Bad.Account>)[] subjects =
        [
            ("Checking", () => new Bad.CheckingAccount(100)),
            ("FixedTermDeposit", () => new Bad.FixedTermDeposit(100)),
            ("PayAsYouGo", () => new Bad.PayAsYouGoAccount(100)),
        ];

        (string, Func<Bad.Account, bool>)[] contract =
        [
            ("Withdraw(30) is accepted when Balance is 100", a => { a.Withdraw(30); return true; }),
            ("Balance drops by exactly the amount", a => { var b = a.Balance; a.Withdraw(30); return a.Balance == b - 30; }),
            ("Withdraw then Deposit the same amount restores Balance", a => { var b = a.Balance; a.Withdraw(30); a.Deposit(30); return a.Balance == b; }),
        ];

        log.RunContract("Account.Withdraw contract", subjects, contract);

        log.Header("MonthlyFeeJob.ChargeAll(fee: 5) over all three accounts");
        var accounts = subjects.Select(s => s.Item2()).ToList();
        try
        {
            Bad.MonthlyFeeJob.ChargeAll(accounts, 5);
            log.Ok("all accounts charged");
        }
        catch (Exception ex)
        {
            log.Fail($"job crashed: {ex.GetType().Name}: {ex.Message}");
            log.Warn($"Balances now: {string.Join(", ", accounts.Select(a => a.Balance.ToString("0.00")))} - charged some, not others.");
        }
    }

    public static void RunGood(DemoLog log)
    {
        (string, Func<Good.IWithdrawable>)[] subjects =
        [
            ("Checking", () => new Good.CheckingAccount(100)),
            ("PayAsYouGo", () => new Good.PayAsYouGoAccount(100)),
            ("Overdraft(50)", () => new Good.OverdraftAccount(100, 50)),
        ];

        (string, Func<Good.IWithdrawable, bool>)[] contract =
        [
            ("Withdraw(30) is accepted when Balance is 100", a => { a.Withdraw(30); return true; }),
            ("Balance drops by exactly amount + FeeFor(amount)", a => { var b = a.Balance; var fee = a.FeeFor(30); a.Withdraw(30); return a.Balance == b - 30 - fee; }),
            ("Withdraw beyond Balance + OverdraftLimit is rejected", a => { try { a.Withdraw(a.Balance + a.OverdraftLimit + 1); return false; } catch (ArgumentOutOfRangeException) { return true; } }),
        ];

        log.RunContract("IWithdrawable contract", subjects, contract);

        log.Header("MonthlyFeeJob.ChargeAll(fee: 5)");
        var accounts = subjects.Select(s => s.Item2()).ToList();
        Good.MonthlyFeeJob.ChargeAll(accounts, 5);
        log.Ok($"all withdrawable accounts charged - balances: {string.Join(", ", accounts.Select(a => a.Balance.ToString("0.00")))}");
        log.Ok("new FixedTermDeposit(...) is not IWithdrawable: passing it to ChargeAll doesn't compile.");
        log.Muted("OverdraftAccount accepts MORE than its base (weaker precondition) - still substitutable.");
    }
}
