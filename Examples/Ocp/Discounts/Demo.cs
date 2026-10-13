using SolidShowdown.Catalog;
using BadAfter = SolidShowdown.Examples.Ocp.Discounts.Bad.AfterStudent;
using Good = SolidShowdown.Examples.Ocp.Discounts.Good;

namespace SolidShowdown.Examples.Ocp.Discounts;

public static class Demo
{
    // The spec Product gave us for the new customer kind.
    private const decimal ExpectedStudentPrice = 85m;
    private const int ExpectedStudentPoints = 85;

    public static void RunBad(DemoLog log)
    {
        log.Header("After adding Student by editing the enum + switches (100.00 base price)");
        var price = new BadAfter.PriceCalculator();
        var loyalty = new BadAfter.LoyaltyCalculator();
        foreach (var type in Enum.GetValues<BadAfter.CustomerType>())
        {
            var paid = price.Price(100m, type);
            log.Info($"{type,-8} pays {paid,6:0.00}  earns {loyalty.Points(paid, type),3} points");
        }

        log.Header("Acceptance test for the Student feature");
        var studentPaid = price.Price(100m, BadAfter.CustomerType.Student);
        log.Check("student pays 85.00", () => studentPaid == ExpectedStudentPrice);
        log.Check("student earns 85 points", () => loyalty.Points(studentPaid, BadAfter.CustomerType.Student) == ExpectedStudentPoints);
        log.Muted("LoyaltyCalculator wasn't updated. It compiled, because `_ => 0` swallowed the new case.");
    }

    public static void RunGood(DemoLog log)
    {
        log.Header("After adding Student as a new class (100.00 base price)");
        var checkout = new Good.Checkout();
        Good.ICustomerPricing[] customers =
            [new Good.RegularPricing(), new Good.PremiumPricing(), new Good.StaffPricing(), new Good.StudentPricing()];
        foreach (var c in customers)
        {
            var (paid, points) = checkout.Pay(100m, c);
            log.Info($"{c.Name,-8} pays {paid,6:0.00}  earns {points,3} points");
        }

        log.Header("Acceptance test for the Student feature");
        var (studentPaid, studentPoints) = checkout.Pay(100m, new Good.StudentPricing());
        log.Check("student pays 85.00", () => studentPaid == ExpectedStudentPrice);
        log.Check("student earns 85 points", () => studentPoints == ExpectedStudentPoints);
        log.Muted("Checkout, RegularPricing, PremiumPricing and StaffPricing were not touched.");
    }
}
