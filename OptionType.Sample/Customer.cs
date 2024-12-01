namespace OptionType.Sample;

public class Customer
{
    public int Id { get; }
    public string Name { get; }
    public Option<Address> ShippingAddress { get; }
    public Option<string> Email { get; }

    public Customer(int id, string name, Option<Address> shippingAddress, Option<string> email)
    {
        Id = id;
        Name = name;
        ShippingAddress = shippingAddress;
        Email = email;
    }

    public Option<string> GetShippingLabel() =>
        ShippingAddress.Map(address => $"{Name}\n{address}");

    public Option<string> GetEmailConfirmation() =>
        Email.Bind(email => IsValidEmail(email)
            ? Option<string>.Some($"Order confirmation will be sent to: {email}")
            : Option<string>.None());

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        
        return Option<string>.Some(email)
            .Bind(e => {
                var parts = e.Split('@');
                return parts.Length == 2 && 
                       !string.IsNullOrWhiteSpace(parts[0]) && 
                       !string.IsNullOrWhiteSpace(parts[1]) &&
                       parts[1].Contains('.')
                    ? Option<string>.Some(e)
                    : Option<string>.None();
            })
            .IsSome;
    }
}
