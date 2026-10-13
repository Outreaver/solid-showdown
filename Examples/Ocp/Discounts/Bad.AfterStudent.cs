namespace SolidShowdown.Examples.Ocp.Discounts.Bad.AfterStudent;

// Request: "Add Student customers: 15% off, 1 point per unit spent."
// The developer modifies the enum and the PriceCalculator...

public enum CustomerType { Regular, Premium, Staff, Student }

public sealed class PriceCalculator
{
    public decimal Price(decimal basePrice, CustomerType type) => type switch
    {
        CustomerType.Regular => basePrice,
        CustomerType.Premium => basePrice * 0.90m,
        CustomerType.Staff => basePrice * 0.70m,
        CustomerType.Student => basePrice * 0.85m, // edited
        _ => basePrice,
    };
}

// ❌ ...and forgets this one. It compiles. It ships. Students silently get 0 points.
public sealed class LoyaltyCalculator
{
    public int Points(decimal paid, CustomerType type) => type switch
    {
        CustomerType.Regular => (int)paid,
        CustomerType.Premium => (int)(paid * 2),
        CustomerType.Staff => 0,
        _ => 0,
    };
}
