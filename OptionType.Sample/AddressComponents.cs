using Result;

namespace OptionType.Sample;

public record Street
{
    public string Value { get; }

    private Street(string value)
    {
        Value = value;
    }

    public static Result<Street> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<Street>.Error("Invalid street");
        if (value.Length > 100)
            return Result<Street>.Error("Street cannot be longer than 100 characters.");
        
        return Result<Street>.Success(new Street(value));
    }

    public override string ToString() => Value;
}

public record City
{
    public string Value { get; }

    private City(string value)
    {
        Value = value;
    }

    public static Result<City> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<City>.Error("Invalid city");
        if (value.Length > 50)
            return Result<City>.Error("City cannot be longer than 50 characters.");
        
        return Result<City>.Success(new City(value));
    }

    public override string ToString() => Value;
}

public record PostalCode
{
    public string Value { get; }

    private PostalCode(string value)
    {
        Value = value;
    }

    public static Result<PostalCode> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<PostalCode>.Error("Invalid postal code");
        if (value.Length > 10)
            return Result<PostalCode>.Error("Postal code cannot be longer than 10 characters.");
        if (!value.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '-'))
            return Result<PostalCode>.Error("Invalid postal code");
        
        return Result<PostalCode>.Success(new PostalCode(value));
    }

    public override string ToString() => Value;
}

public record BuildingNumber
{
    public string Value { get; }

    private BuildingNumber(string value)
    {
        Value = value;
    }

    public static Result<BuildingNumber> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<BuildingNumber>.Error("Invalid building number");
        if (value.Length > 10)
            return Result<BuildingNumber>.Error("Building number cannot be longer than 10 characters.");
        
        return Result<BuildingNumber>.Success(new BuildingNumber(value));
    }

    public override string ToString() => Value;
}

public record Country
{
    public string Value { get; }

    private Country(string value)
    {
        Value = value;
    }

    public static Result<Country> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<Country>.Error("Invalid country");
        if (value.Length > 50)
            return Result<Country>.Error("Country cannot be longer than 50 characters.");
        if (!value.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
            return Result<Country>.Error("Invalid country");
        
        return Result<Country>.Success(new Country(value));
    }

    public override string ToString() => Value;
}
