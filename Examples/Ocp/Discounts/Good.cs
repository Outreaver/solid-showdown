namespace SolidShowdown.Examples.Ocp.Discounts.Good;

// ✅ FOLLOWS OCP (for one axis of change): everything that varies per customer kind
//    lives behind one abstraction. A new kind of customer = a new class.
//    Checkout is CLOSED for modification but the system is OPEN for extension.
public interface ICustomerPricing
{
    string Name { get; }
    decimal Price(decimal basePrice);
    int LoyaltyPoints(decimal paid);
}

public sealed class RegularPricing : ICustomerPricing
{
    public string Name => "Regular";
    public decimal Price(decimal basePrice) => basePrice;
    public int LoyaltyPoints(decimal paid) => (int)paid;
}

public sealed class PremiumPricing : ICustomerPricing
{
    public string Name => "Premium";
    public decimal Price(decimal basePrice) => basePrice * 0.90m;
    public int LoyaltyPoints(decimal paid) => (int)(paid * 2);
}

public sealed class StaffPricing : ICustomerPricing
{
    public string Name => "Staff";
    public decimal Price(decimal basePrice) => basePrice * 0.70m;
    public int LoyaltyPoints(decimal paid) => 0;
}

// Never edited when a new customer kind appears.
public sealed class Checkout
{
    public (decimal Paid, int Points) Pay(decimal basePrice, ICustomerPricing customer)
    {
        var paid = customer.Price(basePrice);
        return (paid, customer.LoyaltyPoints(paid));
    }
}

// 🤔 CONTEXT: "closed" is only ever closed AGAINST SOMETHING.
//    This design is closed against "new kinds of customer". It is NOT closed against
//    "each customer kind gets its own cart-based discount" (Premium: spend 500, get 10% off) -
//    Price(decimal basePrice) simply doesn't receive the cart. (A flat cart discount for
//    everyone could live in Checkout as a separate rule; it's the per-kind part that hurts.) That request forces us to MODIFY the interface and
//    every implementation. No class can be open to every possible extension; trying to
//    anticipate everything produces speculative, hard-to-read abstractions.
//    Practical rule ("fool me once"): write the simple version first; when the first change
//    of a given KIND arrives, refactor so that the next change of that kind is an extension.
