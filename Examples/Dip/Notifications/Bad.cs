namespace SolidShowdown.Examples.Dip.Notifications.Bad;

public sealed record Order(int Id, string CustomerEmail, decimal Total);

// LOW-LEVEL detail: talks to a specific mail server. (Simulated: in this sandbox there is
// no SMTP server, exactly like on a CI agent or a developer laptop.)
public sealed class SmtpEmailSender
{
    public string Host => "smtp.company.local:25";

    public void Send(string to, string subject, string body) =>
        throw new InvalidOperationException($"Could not connect to {Host}");
}

// ❌ BREAKS DIP: HIGH-LEVEL policy ("an order over 0 is accepted and the customer is told")
//    depends directly on a LOW-LEVEL detail and even creates it with `new`.
//    Source-code dependency:   OrderService ───▶ SmtpEmailSender
//    - can't place an order (or test the rules) without a reachable SMTP server
//    - switching to SMS / SendGrid / a queue means editing business logic
//    - the business module can't be compiled or shipped without the mail library
public sealed class OrderService
{
    private readonly SmtpEmailSender _email = new(); // ❌ hard-wired

    public List<Order> Accepted { get; } = [];

    public void PlaceOrder(Order order)
    {
        if (order.Total <= 0) throw new ArgumentException("Order total must be positive.");
        Accepted.Add(order);
        _email.Send(order.CustomerEmail, $"Order #{order.Id}", $"Thanks! Total: {order.Total:0.00}");
    }
}
