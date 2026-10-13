using SolidShowdown.Catalog;
using Bad = SolidShowdown.Examples.Dip.Notifications.Bad;
using Good = SolidShowdown.Examples.Dip.Notifications.Good;

namespace SolidShowdown.Examples.Dip.Notifications;

public static class Demo
{
    public static void RunBad(DemoLog log)
    {
        log.Header("Unit test: a valid order is accepted");
        var service = new Bad.OrderService();
        log.Check("PlaceOrder(#1, 49.99) adds the order", () =>
        {
            service.PlaceOrder(new(1, "ann@example.com", 49.99m));
            return service.Accepted.Count == 1;
        });
        log.Warn("The business rule worked (Accepted.Count is now " + service.Accepted.Count + "), but the test died on e-mail.");

        log.Header("Product asks: 'notify by SMS instead'");
        log.Fail("No way to do it without editing OrderService - the sender is created inside it.");
    }

    public static void RunGood(DemoLog log)
    {
        log.Header("Unit test: a valid order is accepted (with an in-memory notifier)");
        var fake = new Good.InMemoryOrderNotifier();
        var service = new Good.OrderService(fake);
        log.Check("PlaceOrder(#1, 49.99) adds the order", () =>
        {
            service.PlaceOrder(new(1, "ann@example.com", 49.99m));
            return service.Accepted.Count == 1;
        });
        log.Check("the customer was notified exactly once", () => fake.Notified.Count == 1);

        log.Header("Product asks: 'notify by SMS instead'");
        var sms = new List<string>();
        var smsService = new Good.OrderService(new Good.SmsOrderNotifier(sms));
        smsService.PlaceOrder(new(2, "bob@example.com", 10m));
        log.Ok("Swapped at the composition root. OrderService unchanged.");
        log.Info(sms[0]);

        log.Header("Production wiring with SMTP (still unreachable here)");
        log.Check("PlaceOrder with SmtpOrderNotifier", () =>
        {
            new Good.OrderService(new Good.SmtpOrderNotifier()).PlaceOrder(new(3, "cy@example.com", 5m));
            return true;
        });
        log.Muted("The detail still fails without a server - DIP doesn't fix infrastructure.");
        log.Muted("It makes the business rules independent of it, so they can be tested and changed alone.");
    }
}
