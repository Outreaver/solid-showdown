using System.Reflection;
using SolidShowdown.Catalog;
using Bad = SolidShowdown.Examples.Isp.Repository.Bad;
using Good = SolidShowdown.Examples.Isp.Repository.Good;

namespace SolidShowdown.Examples.Isp.Repository;

public static class Demo
{
    public static void RunBad(DemoLog log)
    {
        var dependency = typeof(Bad.IRepository<Bad.Order>);
        Describe(log, "MonthlyReport", dependency, used: ["List"], sameRole: ["Get"]);

        log.Header("The audit log under a CRUD interface");
        Bad.IRepository<Bad.AuditEntry> audit = new Bad.AuditLogStore();
        audit.Add(new(1, "user 42 logged in"));
        log.Check("Delete(1) on the audit store is rejected at COMPILE time", () => { audit.Delete(1); return false; });
        log.Muted("It's rejected at runtime instead - after someone wrote, compiled and shipped the call.");
    }

    public static void RunGood(DemoLog log)
    {
        var dependency = typeof(Good.IReader<Good.Order>);
        Describe(log, "MonthlyReport", dependency, used: ["List"], sameRole: ["Get"]);

        log.Header("The audit log with role interfaces");
        var audit = new Good.AuditLogStore();
        audit.Add(new(1, "user 42 logged in"));
        log.Check("AuditLogStore has no Delete/Update to call", () => typeof(Good.AuditLogStore).GetMethod("Delete") is null);
        log.Muted("`audit.Delete(1)` would be a compile error.");

        var orders = new Good.OrderRepository();
        orders.Add(new(1, 120m, new(2026, 9, 3)));
        orders.Add(new(2, 80m, new(2026, 9, 21)));
        log.Info($"Revenue for 2026-09: {new Good.MonthlyReport(orders).Revenue(2026, 9):0.00}");
    }

    // Unused members in the client's own role (Get, for a reader) aren't counted as noise:
    // ISP doesn't demand a perfect fit, only no unrelated baggage. Same rule for both designs.
    private static void Describe(DemoLog log, string client, Type dependency, string[] used, string[] sameRole)
    {
        var members = dependency.GetInterfaces().Prepend(dependency)
            .SelectMany(i => i.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            .Select(m => m.Name).Distinct().ToArray();

        log.Header($"{client} depends on {Pretty(dependency)}");
        var table = log.Table($"Members visible to {client}", "Member", "Used by the client?", "Test double must implement?");
        foreach (var m in members)
        {
            var isUsed = used.Contains(m);
            var noise = !isUsed && !sameRole.Contains(m);
            table.Rows.Add([new(m), isUsed ? new("yes", LineKind.Ok) : new("no", noise ? LineKind.Warn : LineKind.Muted), new("yes", noise ? LineKind.Warn : LineKind.Info)]);
        }

        var noisy = members.Where(m => !used.Contains(m) && !sameRole.Contains(m)).ToArray();
        var idle = members.Where(m => !used.Contains(m) && sameRole.Contains(m)).ToArray();
        if (noisy.Length == 0) log.Ok($"{client} sees {members.Length} member(s), uses {used.Length}, and nothing it sees is unrelated.");
        else log.Warn($"{client} sees {members.Length} members, uses {used.Length}. {string.Join("/", noisy)} are unrelated write-side noise it is still coupled to.");
        if (idle.Length > 0) log.Muted($"{string.Join("/", idle)} is unused today, but belongs to the same cohesive \"read\" role, so it isn't counted as noise.");

        var canDelete = members.Contains("Delete");
        log.Check($"{client} cannot delete orders, even by mistake", () => !canDelete);
    }

    private static string Pretty(Type t) =>
        t.IsGenericType ? $"{t.Name[..t.Name.IndexOf('`')]}<{string.Join(", ", t.GetGenericArguments().Select(a => a.Name))}>" : t.Name;
}
