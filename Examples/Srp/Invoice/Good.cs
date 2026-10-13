using System.Text;

namespace SolidShowdown.Examples.Srp.Invoice.Good;

public sealed record InvoiceLine(string Product, decimal NetPrice, int Quantity);

public sealed record Invoice(string Number, string CustomerEmail, IReadOnlyList<InvoiceLine> Lines);

public sealed class Outbox
{
    public List<string> SentEmails { get; } = [];
}

// ✅ Finance/Legal own this. Changes when tax law or rounding rules change.
public sealed class TaxCalculator
{
    private const decimal VatRate = 0.23m;

    public decimal Total(Invoice invoice) =>
        Math.Round(invoice.Lines.Sum(l => l.NetPrice * l.Quantity) * (1 + VatRate), 2);
}

// ✅ Marketing owns this. Changes when the brand / layout changes.
public sealed class InvoiceRenderer
{
    public string Render(Invoice invoice, decimal total)
    {
        var html = new StringBuilder();
        html.Append($"<h1>ACME Corp</h1><h2>Invoice {invoice.Number}</h2><ul>");
        foreach (var line in invoice.Lines)
            html.Append($"<li>{line.Quantity} × {line.Product}: {line.NetPrice:0.00}</li>");
        html.Append($"</ul><p><b>Total: {total:0.00}</b></p>");
        return html.ToString();
    }
}

// ✅ IT Operations owns this. Changes when we switch mail provider, add retries, etc.
public sealed class InvoiceMailer(Outbox outbox)
{
    public void Send(string to, string body) => outbox.SentEmails.Add($"To {to}: {body}");
}

// ✅ The workflow: "calculate, render, send". It has a reason to change too - when the
//    PROCESS changes (e.g. "also archive a PDF"). That's fine: one reason, one owner (the product team).
public sealed class InvoiceWorkflow(TaxCalculator tax, InvoiceRenderer renderer, InvoiceMailer mailer)
{
    public decimal Process(Invoice invoice)
    {
        var total = tax.Total(invoice);
        mailer.Send(invoice.CustomerEmail, renderer.Render(invoice, total));
        return total;
    }
}

// 🤔 CONTEXT: don't keep slicing. Splitting InvoiceRenderer into HeaderRenderer,
//    LineRenderer and FooterRenderer would NOT be "more SRP" - the same person (Marketing)
//    asks for all of those changes. SRP tells you where to cut (between actors),
//    not to cut as small as possible. Cohesion matters as much as separation.
