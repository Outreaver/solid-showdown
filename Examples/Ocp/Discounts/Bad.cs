namespace SolidShowdown.Examples.Ocp.Discounts.Bad;

public enum CustomerType { Regular, Premium, Staff }

// ❌ BREAKS OCP: every new kind of customer forces us to OPEN and EDIT this class.
//    Worse, the same `switch` on CustomerType is copy-pasted into other classes
//    (see LoyaltyCalculator). Adding one enum value means hunting down every switch -
//    "shotgun surgery" - and the compiler won't tell you which ones you missed.
public sealed class PriceCalculator
{
    public decimal Price(decimal basePrice, CustomerType type) => type switch
    {
        CustomerType.Regular => basePrice,
        CustomerType.Premium => basePrice * 0.90m,
        CustomerType.Staff => basePrice * 0.70m,
        _ => basePrice, // ❌ the silent fallback that hides forgotten cases
    };
}

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

// 🤔 CONTEXT: a switch over a closed, stable set is NOT automatically an OCP violation.
//    `switch (dayOfWeek)` is fine - nobody will add an 8th day. The question is:
//    "Is this set of cases a thing the business keeps extending?" Customer tiers usually are.
//    If you've had 3 tiers for 5 years and nobody asks for more, this code is simpler and OK.
