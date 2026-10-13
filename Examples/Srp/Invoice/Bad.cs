using System.Text;

namespace SolidShowdown.Examples.Srp.Invoice.Bad;

public sealed record InvoiceLine(string Product, decimal NetPrice, int Quantity);

public sealed record Invoice(string Number, string CustomerEmail, IReadOnlyList<InvoiceLine> Lines);

// Something that records what "the outside world" saw - stands in for a real SMTP client.
public sealed class Outbox
{
    public List<string> SentEmails { get; } = [];
}

// ❌ BREAKS SRP: the tax rules (Finance/Legal), the look of the document (Marketing)
//    and the delivery mechanism (IT Operations) all live in one class.
//    Every one of those groups has a reason to edit this file, and every edit
//    redeploys - and should re-test - the other two people's logic.
public sealed class InvoiceService(Outbox outbox)
{
    private const decimal VatRate = 0.23m;

    public decimal Process(Invoice invoice)
    {
        // --- Finance: how money is calculated ---
        var net = invoice.Lines.Sum(l => l.NetPrice * l.Quantity);
        var total = Math.Round(net * (1 + VatRate), 2);

        // --- Marketing: how the document looks ---
        var html = new StringBuilder();
        html.Append($"<h1>ACME Corp</h1><h2>Invoice {invoice.Number}</h2><ul>");
        foreach (var line in invoice.Lines)
            html.Append($"<li>{line.Quantity} × {line.Product}: {line.NetPrice:0.00}</li>");
        html.Append($"</ul><p><b>Total: {total:0.00}</b></p>");

        // --- IT Ops: how it is delivered ---
        outbox.SentEmails.Add($"To {invoice.CustomerEmail}: {html}");

        // ❌ Consequence you can feel in tests: there is no way to ask
        //    "what is the total?" without also rendering HTML and sending an e-mail.
        return total;
    }
}
