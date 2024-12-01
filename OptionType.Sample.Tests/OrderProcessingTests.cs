using Xunit;

namespace OptionType.Sample.Tests;

public class OrderProcessingTests
{
    private static Customer CreateCustomer(bool includeNumber = true, bool includeCountry = true, Option<string>? email = null)
    {
        var addressResult = Address.Create(
            "123 Main St",
            "Springfield",
            "12345",
            includeNumber ? Option<string>.Some("4B") : Option<string>.None(),
            includeCountry ? Option<string>.Some("USA") : Option<string>.None()
        );

        Assert.True(addressResult.IsSuccess, $"Address creation failed: {addressResult.GetError()}");
        Assert.NotNull(addressResult.Value);
        
        return new Customer(1, "John Doe", Option<Address>.Some(addressResult.Value), email ?? Option<string>.None());
    }

    private static Discount CreateDiscount(decimal percentage = 20m, 
        Option<DateTime>? startDate = default, 
        Option<DateTime>? endDate = default,
        Option<DayOfWeek>? dayOfWeek = default)
    {
        var result = Discount.Create(
            percentage,
            startDate ?? Option<DateTime>.None(),
            endDate ?? Option<DateTime>.None(),
            dayOfWeek ?? Option<DayOfWeek>.None()
        );
        
        Assert.True(result.IsSuccess, $"Discount creation failed: {result.GetError()}");
        Assert.NotNull(result.Value);
        
        return result.Value;
    }

    [Fact]
    public void Customer_WithFullAddress_GeneratesCompleteShippingLabel()
    {
        var customer = CreateCustomer();
        var label = customer.GetShippingLabel().Unwrap(() => "");
        
        Assert.Contains("Number 4B", label);
        Assert.Contains("John Doe", label);
        Assert.Contains("123 Main St", label);
    }

    [Fact]
    public void Customer_WithoutNumber_GeneratesBasicShippingLabel()
    {
        var customer = CreateCustomer(includeNumber: false);
        var label = customer.GetShippingLabel().Unwrap(() => "");
        
        Assert.DoesNotContain("Number", label);
        Assert.Contains("John Doe", label);
        Assert.Contains("123 Main St", label);
    }

    [Fact]
    public void Customer_WithEmail_GeneratesEmailConfirmation()
    {
        var customer = CreateCustomer(email: Option<string>.Some("john@example.com"));
        var confirmation = customer.GetEmailConfirmation().Unwrap(() => "");
        
        Assert.Contains("john@example.com", confirmation);
    }

    [Fact]
    public void Customer_WithoutEmail_ReturnsNone()
    {
        var customer = CreateCustomer();
        var confirmation = customer.GetEmailConfirmation();
        
        Assert.False(confirmation.IsSome);
    }

    [Fact]
    public void Order_WithDiscount_CalculatesDiscountedTotal()
    {
        var customer = CreateCustomer();
        var discount = CreateDiscount();
        var order = new Order(1, customer, 100m, Option<Discount>.Some(discount));
        
        Assert.Equal(80m, order.CalculateFinalTotal());
    }

    [Fact]
    public void Order_WithoutDiscount_UsesOriginalTotal()
    {
        var customer = CreateCustomer();
        var order = new Order(1, customer, 100m, Option<Discount>.None());
        
        Assert.Equal(100m, order.CalculateFinalTotal());
    }

    [Fact]
    public void Order_WithFullDetails_GeneratesCompleteInvoice()
    {
        var customer = CreateCustomer();
        var discount = CreateDiscount();
        var order = new Order(1, customer, 100m, Option<Discount>.Some(discount));
        
        var invoice = order.GenerateInvoice().Unwrap(() => "");
        
        Assert.Contains("Invoice #1", invoice);
        Assert.Contains("John Doe", invoice);
        Assert.Contains("Number 4B", invoice);
        Assert.Contains("Discount: 20%", invoice);
        Assert.Contains("Final Total: $80.00", invoice);
    }

    [Fact]
    public void Order_WithoutDiscount_GeneratesBasicInvoice()
    {
        var customer = CreateCustomer();
        var order = new Order(1, customer, 100m, Option<Discount>.None());
        
        var invoice = order.GenerateInvoice().Unwrap(() => "");
        
        Assert.Contains("Invoice #1", invoice);
        Assert.Contains("John Doe", invoice);
        Assert.Contains("Number 4B", invoice);
        Assert.DoesNotContain("Discount", invoice);
        Assert.Contains("Total: $100.00", invoice);
    }

    [Fact]
    public void Discount_WithFutureEndDate_IsValid()
    {
        var futureDate = DateTime.Now.AddDays(1);
        var discount = CreateDiscount(endDate: Option<DateTime>.Some(futureDate));

        Assert.True(discount.IsValid());
    }

    [Fact]
    public void Discount_WithPastEndDate_IsNotValid()
    {
        var pastDate = DateTime.Now.AddDays(-1);
        var discount = CreateDiscount(endDate: Option<DateTime>.Some(pastDate));

        Assert.False(discount.IsValid());
    }

    [Fact]
    public void Discount_WithoutDates_IsAlwaysValid()
    {
        var discount = CreateDiscount();

        Assert.True(discount.IsValid());
    }

    [Fact]
    public void Discount_WithFutureStartDate_IsNotValid()
    {
        var futureDate = DateTime.Now.AddDays(1);
        var discount = CreateDiscount(startDate: Option<DateTime>.Some(futureDate));

        Assert.False(discount.IsValid());
    }

    [Fact]
    public void Discount_WithPastStartDate_IsValid()
    {
        var pastDate = DateTime.Now.AddDays(-1);
        var discount = CreateDiscount(startDate: Option<DateTime>.Some(pastDate));

        Assert.True(discount.IsValid());
    }

    [Fact]
    public void Discount_WithValidDateRange_IsValid()
    {
        var startDate = DateTime.Now.AddDays(-1);
        var endDate = DateTime.Now.AddDays(1);
        var discount = CreateDiscount(
            startDate: Option<DateTime>.Some(startDate),
            endDate: Option<DateTime>.Some(endDate)
        );

        Assert.True(discount.IsValid());
    }

    [Fact]
    public void Discount_WithInvalidDateRange_IsNotValid()
    {
        var startDate = DateTime.Now.AddDays(1);
        var endDate = DateTime.Now.AddDays(2);
        var discount = CreateDiscount(
            startDate: Option<DateTime>.Some(startDate),
            endDate: Option<DateTime>.Some(endDate)
        );

        Assert.False(discount.IsValid());
    }

    [Fact]
    public void Order_WithExpiredDiscount_StillAppliesDiscount()
    {
        var pastDate = DateTime.Now.AddDays(-2);
        var endDate = DateTime.Now.AddDays(-1);
        var customer = CreateCustomer();
        var discount = CreateDiscount(
            startDate: Option<DateTime>.Some(pastDate),
            endDate: Option<DateTime>.Some(endDate)
        );
        var order = new Order(1, customer, 100m, Option<Discount>.Some(discount));

        Assert.False(discount.IsValid());
        Assert.Equal(80m, order.CalculateFinalTotal());
    }

    [Fact]
    public void Order_WithZeroTotal_HandlesDiscountCorrectly()
    {
        var customer = CreateCustomer();
        var discount = CreateDiscount();
        var order = new Order(1, customer, 0m, Option<Discount>.Some(discount));

        Assert.Equal(0m, order.CalculateFinalTotal());
    }

    [Theory]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(75)]
    [InlineData(100)]
    public void Discount_WithVariousPercentages_CalculatesCorrectly(decimal percentage)
    {
        var discount = CreateDiscount(percentage: percentage);
        var order = new Order(1, CreateCustomer(), 100m, Option<Discount>.Some(discount));

        var expectedTotal = 100m * (1 - percentage / 100m);
        Assert.Equal(expectedTotal, order.CalculateFinalTotal());
    }

    [Fact]
    public void Address_WithCountry_IncludesCountryInLabel()
    {
        var customer = CreateCustomer(includeCountry: true);
        var label = customer.GetShippingLabel().Unwrap(() => "");
        
        Assert.Contains("12345 USA", label);
    }

    [Fact]
    public void Address_WithoutCountry_ExcludesCountryFromLabel()
    {
        var customer = CreateCustomer(includeCountry: false);
        var label = customer.GetShippingLabel().Unwrap(() => "");
        
        Assert.DoesNotContain(" USA", label);
    }

    [Fact]
    public void Customer_WithMultipleOptionsNone_HandlesGracefully()
    {
        var customer = new Customer(1, "John Doe", Option<Address>.None(), Option<string>.None());

        var shippingLabel = customer.GetShippingLabel();
        var emailConfirmation = customer.GetEmailConfirmation();

        Assert.False(shippingLabel.IsSome);
        Assert.False(emailConfirmation.IsSome);
    }

    [Fact]
    public void Customer_WithInvalidEmail_HandlesGracefully()
    {
        var addressResult = Address.Create(
            "123 Main St",
            "Springfield",
            "12345",
            Option<string>.Some("USA"),
            Option<string>.Some("USA")
        );
        Assert.True(addressResult.IsSuccess, $"Address creation failed: {addressResult.GetError()}");
        Assert.NotNull(addressResult.Value);
        
        var customer = new Customer(1, "John Doe", 
            Option<Address>.Some(addressResult.Value), 
            Option<string>.Some("invalid-email"));

        var result = customer.GetEmailConfirmation();

        Assert.False(result.IsSome);
    }

    [Fact]
    public void Address_WithLongStreetName_FormatsCorrectly()
    {
        var longStreet = "12345 Very Long Street Name That Could Potentially Cause Formatting Issues";
        var addressResult = Address.Create(
            longStreet,
            "Springfield",
            "12345",
            Option<string>.Some("4B"),
            Option<string>.None()
        );
        Assert.True(addressResult.IsSuccess, $"Address creation failed: {addressResult.GetError()}");
        Assert.NotNull(addressResult.Value);
        
        var customer = new Customer(1, "John Doe", Option<Address>.Some(addressResult.Value), Option<string>.None());

        var label = customer.GetShippingLabel().Unwrap(() => "");

        Assert.Contains(longStreet, label);
        Assert.Contains("Number 4B", label);
    }

    [Fact]
    public void ChainedOptions_HandleNoneGracefully()
    {
        var addressResult = Address.Create(
            "123 Main St",
            "Springfield",
            "12345",
            Option<string>.None(),
            Option<string>.None()
        );
        Assert.True(addressResult.IsSuccess, $"Address creation failed: {addressResult.GetError()}");
        Assert.NotNull(addressResult.Value);
        
        var customer = new Customer(1, "John Doe", 
            Option<Address>.Some(addressResult.Value), 
            Option<string>.None());

        var result = customer.Email
            .Map(email => email.ToUpper())
            .Bind(upper => Option<string>.Some($"Email: {upper}"))
            .Map(formatted => formatted.Length)
            .Map(length => length > 0);

        Assert.False(result.IsSome);
    }

    [Fact]
    public void Customer_WithNullEmail_HandlesGracefully()
    {
        string? nullEmail = null;
        var addressResult = Address.Create(
            "123 Main St",
            "Springfield",
            "12345",
            Option<string>.Some("4B"),
            Option<string>.Some("USA")
        );
        Assert.True(addressResult.IsSuccess, $"Address creation failed: {addressResult.GetError()}");
        Assert.NotNull(addressResult.Value);
        
        var customer = new Customer(1, "John Doe", 
            Option<Address>.Some(addressResult.Value), 
            Option<string>.Some(nullEmail!));

        var emailConfirmation = customer.GetEmailConfirmation();

        Assert.False(emailConfirmation.IsSome);
    }

    [Fact]
    public void Address_WithSpecialCharacters_FormatsCorrectly()
    {
        var addressResult = Address.Create(
            "123 Main St. #&@",
            "Spring-Field",
            "12345-6789",
            Option<string>.Some("4B!"),
            Option<string>.None()
        );
        Assert.True(addressResult.IsSuccess, $"Address creation failed: {addressResult.GetError()}");
        Assert.NotNull(addressResult.Value);
        
        var customer = new Customer(1, "John & Jane Doe", Option<Address>.Some(addressResult.Value), Option<string>.None());

        var label = customer.GetShippingLabel().Unwrap(() => "");

        Assert.Contains("123 Main St. #&@", label);
        Assert.Contains("Spring-Field", label);
        Assert.Contains("Number 4B!", label);
    }

    [Fact]
    public void ComplexOptionChain_WithAllSome_ProcessesCorrectly()
    {
        var customer = CreateCustomer(email: Option<string>.Some("john@example.com"));
        var futureDate = DateTime.Now.AddDays(1);
        var discount = CreateDiscount(endDate: Option<DateTime>.Some(futureDate));
        var order = new Order(1, customer, 100m, Option<Discount>.Some(discount));

        var result = order.Customer.Email
            .Bind(email => order.Customer.GetShippingLabel()
                .Map(label => $"Email: {email}, Label: {label}"))
            .Map(combined => combined.Length)
            .Map(length => length > 0);

        Assert.True(result.IsSome);
        Assert.True(result.Unwrap(() => false));
    }

    [Fact]
    public void ComplexOptionChain_WithSomeNone_ShortCircuits()
    {
        var addressResult = Address.Create(
            "123 Main St",
            "Springfield",
            "12345",
            Option<string>.None(),
            Option<string>.None()
        );
        Assert.True(addressResult.IsSuccess, $"Address creation failed: {addressResult.GetError()}");
        Assert.NotNull(addressResult.Value);
        
        var customer = new Customer(1, "John Doe", 
            Option<Address>.Some(addressResult.Value), 
            Option<string>.None());
        var discount = CreateDiscount();
        var order = new Order(1, customer, 100m, Option<Discount>.Some(discount));

        var result = order.Customer.Email
            .Bind(email => order.Customer.GetShippingLabel()
                .Map(label => $"Email: {email}, Label: {label}"))
            .Map(combined => combined.Length)
            .Map(length => length > 0);

        Assert.False(result.IsSome);
    }

    [Fact]
    public void Discount_WithCurrentDayOfWeek_IsValid()
    {
        var today = DateTime.Now.DayOfWeek;
        var discount = CreateDiscount(dayOfWeek: Option<DayOfWeek>.Some(today));

        Assert.True(discount.IsValid());
    }

    [Fact]
    public void Discount_WithDifferentDayOfWeek_IsNotValid()
    {
        var tomorrow = DateTime.Now.AddDays(1).DayOfWeek;
        var discount = CreateDiscount(dayOfWeek: Option<DayOfWeek>.Some(tomorrow));

        Assert.False(discount.IsValid());
    }

    [Fact]
    public void Discount_WithoutDayOfWeek_IsValid()
    {
        var discount = CreateDiscount(dayOfWeek: Option<DayOfWeek>.None());

        Assert.True(discount.IsValid());
    }

    [Fact]
    public void Discount_WithAllOptions_ValidatesCorrectly()
    {
        var now = DateTime.Now;
        var discount = CreateDiscount(
            startDate: Option<DateTime>.Some(now.AddDays(-1)),
            endDate: Option<DateTime>.Some(now.AddDays(1)),
            dayOfWeek: Option<DayOfWeek>.Some(now.DayOfWeek)
        );

        Assert.True(discount.IsValid());
    }

    [Fact]
    public void Order_WithDaySpecificDiscount_CalculatesCorrectly()
    {
        var customer = CreateCustomer();
        var today = DateTime.Now.DayOfWeek;
        var discount = CreateDiscount(dayOfWeek: Option<DayOfWeek>.Some(today));
        var order = new Order(1, customer, 100m, Option<Discount>.Some(discount));

        Assert.Equal(80m, order.CalculateFinalTotal());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(150)]
    public void Discount_WithInvalidPercentage_ReturnsError(decimal percentage)
    {
        var result = Discount.Create(percentage, Option<DateTime>.None(), Option<DateTime>.None());
        
        Assert.True(result.IsError);
        Assert.Contains("between 0 and 100", result.GetError());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void Discount_WithValidPercentage_ReturnsSuccess(decimal percentage)
    {
        var result = Discount.Create(percentage, Option<DateTime>.None(), Option<DateTime>.None());
        
        Assert.True(result.IsSuccess);
        Assert.Equal(percentage, result.Match(
            success: d => d.Percentage,
            failure: _ => 0));
    }

    [Fact]
    public void Address_WithValidData_ShouldCreateSuccessfully()
    {
        var result = Address.Create(
            "123 Main St",
            "Springfield",
            "12345",
            Option<string>.Some("4B"),
            Option<string>.Some("USA")
        );

        Assert.True(result.IsSuccess);
        Assert.Equal("123 Main St", result.Value!.Street.Value);
        Assert.Equal("Springfield", result.Value.City.Value);
        Assert.Equal("12345", result.Value.PostalCode.Value);
        Assert.Equal("4B", result.Value.Number.Map(n => n.Value).Unwrap(() => ""));
        Assert.Equal("USA", result.Value.Country.Map(c => c.Value).Unwrap(() => ""));
    }

    [Theory]
    [InlineData("", "City", "12345", "Invalid street")]
    [InlineData("Street", "", "12345", "Invalid city")]
    [InlineData("Street", "City", "", "Invalid postal code")]
    [InlineData("Street", "City", "ABC123!@#", "Invalid postal code")]
    public void Address_WithInvalidData_ShouldReturnError(string street, string city, string postalCode, string expectedError)
    {
        var result = Address.Create(
            street,
            city,
            postalCode,
            Option<string>.None(),
            Option<string>.None()
        );

        Assert.True(result.IsError);
        Assert.Contains(expectedError, result.GetError());
    }

    [Fact]
    public void Address_WithInvalidBuildingNumber_ShouldReturnError()
    {
        var result = Address.Create(
            "123 Main St",
            "Springfield",
            "12345",
            Option<string>.Some(""), // Invalid building number
            Option<string>.None()
        );

        Assert.True(result.IsError);
        Assert.Contains("Invalid building number", result.GetError());
    }

    [Fact]
    public void Address_WithInvalidCountry_ShouldReturnError()
    {
        var result = Address.Create(
            "123 Main St",
            "Springfield",
            "12345",
            Option<string>.None(),
            Option<string>.Some("123") // Invalid country (contains numbers)
        );

        Assert.True(result.IsError);
        Assert.Contains("Invalid country", result.GetError());
    }
}
