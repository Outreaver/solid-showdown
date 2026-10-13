namespace SolidShowdown.Examples.Ocp.Discounts.Good;

// Request: "Add Student customers: 15% off, 1 point per unit spent."
// ✅ A brand-new file. No existing file was edited.
// ✅ And because ICustomerPricing requires LoyaltyPoints, the compiler FORCES us to
//    decide about points - the "forgotten switch" bug from the bad version can't happen.
public sealed class StudentPricing : ICustomerPricing
{
    public string Name => "Student";
    public decimal Price(decimal basePrice) => basePrice * 0.85m;
    public int LoyaltyPoints(decimal paid) => (int)paid;
}

// Honest footnote: SOMETHING still has to change - the place that decides which
// ICustomerPricing a given customer gets (a factory, a DI registration, a DB lookup).
// OCP doesn't abolish modification; it pushes it to the edges (composition root),
// away from the business logic you most want to keep stable.
