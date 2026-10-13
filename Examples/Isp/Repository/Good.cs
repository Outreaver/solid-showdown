namespace SolidShowdown.Examples.Isp.Repository.Good;

public sealed record Order(int Id, decimal Total, DateOnly Date);
public sealed record AuditEntry(int Id, string Message);

// ✅ Role interfaces, split along how clients actually use the data.
public interface IReader<T>
{
    T? Get(int id);
    IReadOnlyList<T> List();
}

public interface IAppender<T>
{
    void Add(T item);
}

public interface IWriter<T> : IAppender<T>
{
    void Update(T item);
    void Delete(int id);
}

// One class may still implement several roles - ISP is about what CLIENTS see.
public sealed class OrderRepository : IReader<Order>, IWriter<Order>
{
    private readonly Dictionary<int, Order> _orders = [];
    public Order? Get(int id) => _orders.GetValueOrDefault(id);
    public IReadOnlyList<Order> List() => [.. _orders.Values];
    public void Add(Order item) => _orders[item.Id] = item;
    public void Update(Order item) => _orders[item.Id] = item;
    public void Delete(int id) => _orders.Remove(id);
}

// ✅ Append-only is now expressed by the TYPE, not by a runtime exception.
public sealed class AuditLogStore : IReader<AuditEntry>, IAppender<AuditEntry>
{
    private readonly List<AuditEntry> _entries = [];
    public AuditEntry? Get(int id) => _entries.FirstOrDefault(e => e.Id == id);
    public IReadOnlyList<AuditEntry> List() => _entries;
    public void Add(AuditEntry item) => _entries.Add(item);
}

// ✅ Can only read. A test double needs 2 members.
public sealed class MonthlyReport(IReader<Order> orders)
{
    public decimal Revenue(int year, int month) =>
        orders.List().Where(o => o.Date.Year == year && o.Date.Month == month).Sum(o => o.Total);
}

// 🤔 CONTEXT: this is Command-Query Separation applied to interfaces - the same read/write
//    split that CQRS takes further with separate models.
//    But don't go further than your clients need: IGetter<T>, ILister<T>, IAdder<T>, IUpdater<T>,
//    IDeleter<T> - one per method - would satisfy ISP's letter and make the code harder to navigate.
//    If you have one CRUD screen and nothing else, IRepository<T> might be exactly right.
