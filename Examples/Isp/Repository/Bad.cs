namespace SolidShowdown.Examples.Isp.Repository.Bad;

public sealed record Order(int Id, decimal Total, DateOnly Date);
public sealed record AuditEntry(int Id, string Message);

// ❌ BREAKS ISP: one generic "do everything" repository interface used by every client.
public interface IRepository<T>
{
    T? Get(int id);
    IReadOnlyList<T> List();
    void Add(T item);
    void Update(T item);
    void Delete(int id);
}

public sealed class OrderRepository : IRepository<Order>
{
    private readonly Dictionary<int, Order> _orders = [];
    public Order? Get(int id) => _orders.GetValueOrDefault(id);
    public IReadOnlyList<Order> List() => [.. _orders.Values];
    public void Add(Order item) => _orders[item.Id] = item;
    public void Update(Order item) => _orders[item.Id] = item;
    public void Delete(int id) => _orders.Remove(id);
}

// ❌ An audit log must be append-only. The interface forces Update/Delete on it anyway.
public sealed class AuditLogStore : IRepository<AuditEntry>
{
    private readonly List<AuditEntry> _entries = [];
    public AuditEntry? Get(int id) => _entries.FirstOrDefault(e => e.Id == id);
    public IReadOnlyList<AuditEntry> List() => _entries.AsReadOnly(); // not castable back to List<T>
    public void Add(AuditEntry item) => _entries.Add(item);
    public void Update(AuditEntry item) => throw new NotSupportedException("Audit log is append-only");
    public void Delete(int id) => throw new NotSupportedException("Audit log is append-only");
}

// ❌ A read-only client that nevertheless:
//    - can call Delete (nothing in its dependency stops it - least privilege is lost),
//    - needs a test double with 5 members, 3 of which are irrelevant noise,
//    - is affected whenever a write-side method is added or changed.
public sealed class MonthlyReport(IRepository<Order> orders)
{
    public decimal Revenue(int year, int month) =>
        orders.List().Where(o => o.Date.Year == year && o.Date.Month == month).Sum(o => o.Total);
}
