namespace OptionType.Sample;

public class Order
{
    public int Id { get; }
    public Customer Customer { get; }
    public decimal Total { get; }
    public Option<Discount> Discount { get; }

    public Order(int id, Customer customer, decimal total, Option<Discount> discount)
    {
        Id = id;
        Customer = customer;
        Total = total;
        Discount = discount;
    }

    public decimal CalculateFinalTotal() =>
        Discount.Map(d => Total - (Total * d.Percentage / 100))
               .Unwrap(() => Total);

    public Option<string> GenerateInvoice() =>
        Customer.GetShippingLabel()
               .Map(shippingLabel =>
                   $"Invoice #{Id}\n\n" +
                   $"{shippingLabel}\n\n" +
                   $"Total: ${Total.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}\n\n" +
                   Discount.Map(d => $"Discount: {d.Percentage}%\nFinal Total: ${CalculateFinalTotal().ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}")
                          .Unwrap(() => "No discount applied"));
}
