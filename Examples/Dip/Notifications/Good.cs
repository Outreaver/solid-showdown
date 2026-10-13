namespace SolidShowdown.Examples.Dip.Notifications.Good;

// ===================== Domain (high-level) =====================

public sealed record Order(int Id, string CustomerEmail, decimal Total);

// ✅ The abstraction is OWNED by the high-level module and phrased in ITS language
//    ("tell the customer their order was placed"), not in the detail's language
//    ("send an e-mail via SMTP"). That ownership is the "inversion": the detail now
//    depends on the policy's interface, not the other way round.
//    Source-code dependencies:  OrderService ──▶ IOrderNotifier ◀── SmtpOrderNotifier
public interface IOrderNotifier
{
    void OrderPlaced(Order order);
}

public sealed class OrderService(IOrderNotifier notifier)
{
    public List<Order> Accepted { get; } = [];

    public void PlaceOrder(Order order)
    {
        if (order.Total <= 0) throw new ArgumentException("Order total must be positive.");
        Accepted.Add(order);
        notifier.OrderPlaced(order);
    }
}

// ===================== Infrastructure (low-level details) =====================
// In a real solution these live in a separate project that references the domain -
// never the reverse.

public sealed class SmtpOrderNotifier : IOrderNotifier
{
    public void OrderPlaced(Order order) =>
        throw new InvalidOperationException("Could not connect to smtp.company.local:25");
}

public sealed class SmsOrderNotifier(List<string> sentMessages) : IOrderNotifier
{
    public void OrderPlaced(Order order) => sentMessages.Add($"SMS: order #{order.Id} confirmed");
}

// Used by tests - no network, fully deterministic.
public sealed class InMemoryOrderNotifier : IOrderNotifier
{
    public List<Order> Notified { get; } = [];
    public void OrderPlaced(Order order) => Notified.Add(order);
}

// ===================== Composition root =====================
// The ONE place that knows about concrete classes (Program.cs / DI container):
//     services.AddScoped<IOrderNotifier>(_ => new SmsOrderNotifier(sentMessages));
//     services.AddScoped<OrderService>();

// 🤔 CONTEXT: DIP is not "put an interface in front of every class".
//    An interface with exactly one implementation that never changes and is never faked
//    (e.g. IOrderTotalCalculator over pure arithmetic) is just indirection. Apply DIP at
//    boundaries to VOLATILE things: I/O, network, time, randomness, third-party SDKs,
//    anything you'll want to swap or fake. Depending on stable concretions (string,
//    List<T>, your own value objects) is fine.
