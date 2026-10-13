using SolidShowdown.Catalog;
using Bad = SolidShowdown.Examples.Srp.Invoice.Bad;
using Good = SolidShowdown.Examples.Srp.Invoice.Good;

namespace SolidShowdown.Examples.Srp.Invoice;

public static class Demo
{
    // Simulates a unit test written by Finance: "2 × 100.00 net with 23% VAT should be 246.00".
    public static void RunBad(DemoLog log)
    {
        log.Header("Finance's unit test: total for 2 × 100.00 net @ 23% VAT");
        var outbox = new Bad.Outbox();
        var service = new Bad.InvoiceService(outbox);
        var invoice = new Bad.Invoice("INV-1", "real.customer@example.com", [new("Widget", 100m, 2)]);

        var total = service.Process(invoice); // the only way to get at the tax logic
        log.Check($"total == 246.00 (got {total:0.00})", () => total == 246.00m);

        log.Header("Side effects of running that 'unit' test");
        log.Warn($"{outbox.SentEmails.Count} e-mail(s) sent, to {invoice.CustomerEmail}");
        log.Muted(outbox.SentEmails[0]);
        log.Warn("The tax test also exercised HTML rendering and e-mail delivery.");
        log.Warn("A typo in Marketing's HTML could now fail Finance's test - and the other way round.");
    }

    public static void RunGood(DemoLog log)
    {
        log.Header("Finance's unit test: total for 2 × 100.00 net @ 23% VAT");
        var invoice = new Good.Invoice("INV-1", "real.customer@example.com", [new("Widget", 100m, 2)]);
        var total = new Good.TaxCalculator().Total(invoice);
        log.Check($"total == 246.00 (got {total:0.00})", () => total == 246.00m);
        log.Ok("No rendering, no e-mail - only the code Finance owns was executed.");

        log.Header("Marketing's unit test, equally isolated");
        var html = new Good.InvoiceRenderer().Render(invoice, 246.00m);
        log.Check("rendered HTML contains the invoice number", () => html.Contains("INV-1"));

        log.Header("Everything still works end-to-end through the workflow");
        var outbox = new Good.Outbox();
        var workflow = new Good.InvoiceWorkflow(new(), new(), new(outbox));
        workflow.Process(invoice);
        log.Check("one e-mail sent by the real workflow", () => outbox.SentEmails.Count == 1);
    }
}
