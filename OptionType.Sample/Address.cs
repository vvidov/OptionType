using Result;

namespace OptionType.Sample;

public class Address
{
    public Street Street { get; }
    public City City { get; }
    public PostalCode PostalCode { get; }
    public Option<BuildingNumber> Number { get; }
    public Option<Country> Country { get; }

    private Address(Street street, City city, PostalCode postalCode, Option<BuildingNumber> number, Option<Country> country)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
        Number = number;
        Country = country;
    }

    public static Result<Address> Create(
        string street,
        string city,
        string postalCode,
        Option<string> number,
        Option<string> countryName)
    {
        var streetResult = Street.Create(street);
        if (streetResult.IsError)
            return Result<Address>.Error(streetResult.GetError()!);

        var cityResult = City.Create(city);
        if (cityResult.IsError)
            return Result<Address>.Error(cityResult.GetError()!);

        var postalCodeResult = PostalCode.Create(postalCode);
        if (postalCodeResult.IsError)
            return Result<Address>.Error(postalCodeResult.GetError()!);

        var numberResult = number
            .Map<Result<BuildingNumber>>(n => BuildingNumber.Create(n))
            .Match<Result<Option<BuildingNumber>>>(
                some: result => result.Match<Result<Option<BuildingNumber>>>(
                    success: bn => Result<Option<BuildingNumber>>.Success(Option<BuildingNumber>.Some(bn)),
                    failure: error => Result<Option<BuildingNumber>>.Error(error)
                ),
                none: () => Result<Option<BuildingNumber>>.Success(Option<BuildingNumber>.None())
            );
        if (numberResult.IsError)
            return Result<Address>.Error(numberResult.GetError()!);

        var countryResult = countryName
            .Map<Result<Country>>(c => OptionType.Sample.Country.Create(c))
            .Match<Result<Option<Country>>>(
                some: result => result.Match<Result<Option<Country>>>(
                    success: cn => Result<Option<Country>>.Success(Option<Country>.Some(cn)),
                    failure: error => Result<Option<Country>>.Error(error)
                ),
                none: () => Result<Option<Country>>.Success(Option<Country>.None())
            );
        if (countryResult.IsError)
            return Result<Address>.Error(countryResult.GetError()!);

        if (!streetResult.IsSuccess || !cityResult.IsSuccess || !postalCodeResult.IsSuccess || 
            !numberResult.IsSuccess || !countryResult.IsSuccess)
        {
            return Result<Address>.Error(
                streetResult.GetError() ?? 
                cityResult.GetError() ?? 
                postalCodeResult.GetError() ?? 
                numberResult.GetError() ?? 
                countryResult.GetError() ?? 
                "Unknown error"
            );
        }

        return Result<Address>.Success(new Address(
            streetResult.Value!,
            cityResult.Value!,
            postalCodeResult.Value!,
            numberResult.Value!,
            countryResult.Value!
        ));
    }

    public override string ToString()
    {
        var numberStr = Number.Match<string>(
            some: number => $" Number {number}",
            none: () => ""
        );
        var countryStr = Country.Match<string>(
            some: country => $" {country}",
            none: () => ""
        );
        return $"{Street}{numberStr}\n{City} {PostalCode}{countryStr}";
    }
}
