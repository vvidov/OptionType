namespace OptionType.Sample;

public class Address
{
    public string Street { get; }
    public string City { get; }
    public string PostalCode { get; }
    public Option<string> Number { get; }
    public Option<string> Country { get; }

    public Address(string street, string city, string postalCode, Option<string> number, Option<string> country)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
        Number = number;
        Country = country;
    }

    public override string ToString()
    {
        var numberPart = Number.Map(number => $" Number {number}").Unwrap(() => "");
        var countryPart = Country.Map(country => $" {country}").Unwrap(() => "");
        return $"{Street}{numberPart}\n{City} {PostalCode}{countryPart}";
    }
}
