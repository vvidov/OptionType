# OptionType - Functional Programming Types for C#

[![Build and Test](https://github.com/vvidov/OptionType/actions/workflows/build.yml/badge.svg)](https://github.com/vvidov/OptionType/actions/workflows/build.yml)
[![Test Matrix](https://github.com/vvidov/OptionType/actions/workflows/test.yml/badge.svg)](https://github.com/vvidov/OptionType/actions/workflows/test.yml)

A robust implementation of functional programming types (`Option<T>` and `Result<T>`) for C#, designed to improve null safety and error handling in your applications.

## Features

- 🛡️ Null-safe operations with `Option<T>`
- 🎯 Explicit error handling with `Result<T>`
- 🔄 Functional method chaining
- 🧪 Comprehensive test coverage
- 📦 No external dependencies

## Architecture

### Option Type Flow

```mermaid
graph TD
    A[Value] --> B{Is Null?}
    B -->|Yes| C[None]
    B -->|No| D[Some]
    D --> E[Map/Match Operations]
    C --> E
    E --> F[Final Value]
```

### Result Type Flow

```mermaid
graph TD
    A[Operation] --> B{Successful?}
    B -->|Yes| C[Success]
    B -->|No| D[Error]
    C --> E[OnSuccess Operations]
    D --> F[Error Handling]
    E --> G[Final Result]
    F --> G
```

### Domain Model

```mermaid
classDiagram
    class Option~T~ {
        +bool HasValue
        +T Value
        +Match()
        +Map()
    }
    
    class Result~T~ {
        +bool IsSuccess
        +bool IsError
        +T Value
        +string Error
        +OnSuccess()
        +Match()
    }
    
    class Discount {
        +decimal Percentage
        +Option~DateTime~ StartDate
        +Option~DateTime~ EndDate
    }
    
    Discount --> "2" Option~DateTime~
```

## Installation

```bash
dotnet add package OptionType  # Coming soon to NuGet
```

## Usage Examples

### Option Type

```csharp
// Creating Options
var someValue = Option<string>.Some("Hello");
var noValue = Option<string>.None();

// Pattern matching
string result = someValue.Match(
    some: value => $"Got: {value}",
    none: () => "Nothing here"
);

// Chaining operations
var processed = someValue
    .Map(s => s.ToUpper())
    .Filter(s => s.Length > 3);
```

### Result Type

```csharp
// Creating Results
var success = Result<int>.Success(42);
var error = Result<int>.Error("Something went wrong");

// Processing results
var final = success.OnSuccess(value => value * 2)
    .Match(
        success: value => $"Result: {value}",
        failure: error => $"Error: {error}"
    );
```

### Real-world Example

```csharp
public class Discount
{
    public decimal Percentage { get; }
    public Option<DateTime> StartDate { get; }
    public Option<DateTime> EndDate { get; }

    public static Result<Discount> Create(
        decimal percentage, 
        Option<DateTime> startDate, 
        Option<DateTime> endDate)
    {
        if (percentage < 0 || percentage > 100)
            return Result<Discount>.Error("Percentage must be between 0 and 100");

        return Result<Discount>.Success(
            new Discount(percentage, startDate, endDate));
    }
}
```

## Best Practices

1. Use `Option<T>` when:
   - A value might be absent
   - You want to avoid null reference exceptions
   - Working with optional parameters

2. Use `Result<T>` when:
   - An operation might fail
   - You want to avoid throwing exceptions
   - You need to pass error information

3. Always handle both cases:
   - Use `Match()` to handle both Some/None or Success/Error cases
   - Avoid accessing `.Value` directly
   - Chain operations using `Map()`/`OnSuccess()`

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## Testing

Run the test suite:

```bash
dotnet test
```

The project includes comprehensive tests for:
- Core Option type functionality
- Core Result type functionality
- Domain model implementations
- Edge cases and error conditions

## Sample Domain Model

The project includes a practical e-commerce domain model that demonstrates real-world usage of `Option<T>` and `Result<T>` types.

### Domain Model Components

```mermaid
classDiagram
    class Customer {
        +int Id
        +string Name
        +Option~Address~ ShippingAddress
        +Option~string~ Email
        +Option~string~ GetShippingLabel()
        +Option~string~ GetEmailConfirmation()
    }
    
    class Address {
        +string Street
        +string City
        +string PostalCode
        +Option~string~ Number
        +Option~string~ Country
        +string ToString()
    }
    
    class Discount {
        +decimal Percentage
        +Option~DateTime~ StartDate
        +Option~DateTime~ EndDate
        +Option~DayOfWeek~ DayOfWeek
        +bool IsValid()
    }
    
    Customer --> "0..1" Address
```

### Sample Scenarios

1. **Customer Management**
   ```csharp
   // Creating a customer with optional address and email
   var customer = new Customer(
       1,
       "John Doe",
       Option<Address>.Some(new Address(
           "123 Main St",
           "Springfield",
           "12345",
           Option<string>.Some("4B"),
           Option<string>.Some("USA")
       )),
       Option<string>.Some("john@example.com")
   );

   // Generating shipping label with null safety
   Option<string> shippingLabel = customer.GetShippingLabel();
   string label = shippingLabel.Unwrap(() => "No shipping address provided");
   ```

2. **Discount Validation**
   ```csharp
   // Creating a time-limited discount
   var discountResult = Discount.Create(
       20.0m,
       Option<DateTime>.Some(DateTime.Today),
       Option<DateTime>.Some(DateTime.Today.AddDays(30)),
       Option<DayOfWeek>.Some(DayOfWeek.Monday)  // Only valid on Mondays
   );

   var discount = discountResult.Match(
       success: d => d.IsValid() ? d : null,
       failure: error => null
   );
   ```

3. **Address Formatting**
   ```csharp
   var address = new Address(
       "123 Main St",
       "Springfield",
       "12345",
       Option<string>.Some("4B"),
       Option<string>.Some("USA")
   );

   // Generates: "123 Main St Number 4B
   //            Springfield 12345 USA"
   string formatted = address.ToString();
   ```

## Unit Testing Strategy

The project includes extensive unit tests demonstrating different testing approaches for functional types.

### Sample Tests

1. **Customer Tests**
   ```csharp
   [Fact]
   public void Customer_WithFullAddress_GeneratesCompleteShippingLabel()
   {
       var customer = CreateCustomer(includeCountry: true);
       var label = customer.GetShippingLabel().Unwrap(() => "");
       
       Assert.Contains("Number 4B", label);
       Assert.Contains("John Doe", label);
       Assert.Contains("123 Main St", label);
       Assert.Contains("USA", label);
   }

   [Fact]
   public void Customer_WithoutNumber_GeneratesBasicShippingLabel()
   {
       var customer = CreateCustomer(includeNumber: false);
       var label = customer.GetShippingLabel().Unwrap(() => "");
       
       Assert.DoesNotContain("Number", label);
       Assert.Contains("John Doe", label);
   }
   ```

2. **Discount Tests**
   ```csharp
   [Theory]
   [InlineData(-1)]
   [InlineData(101)]
   public void Create_WithInvalidPercentage_ShouldReturnError(decimal percentage)
   {
       var result = Discount.Create(
           percentage,
           Option<DateTime>.None(),
           Option<DateTime>.None()
       );
       Assert.True(result.IsError);
   }

   [Fact]
   public void Discount_WithValidDates_IsValid()
   {
       var discount = CreateDiscount(
           startDate: Option<DateTime>.Some(DateTime.Today.AddDays(-1)),
           endDate: Option<DateTime>.Some(DateTime.Today.AddDays(1))
       );
       Assert.True(discount.IsValid());
   }
   ```

### Test Categories

1. **Null Safety Tests**
   - Optional address components
   - Optional email handling
   - Default value handling

2. **Validation Tests**
   - Discount percentage bounds
   - Date range validation
   - Email format validation

3. **Formatting Tests**
   - Address string representation
   - Shipping label generation
   - Email confirmation messages

4. **Business Logic Tests**
   - Discount validity rules
   - Customer information handling
   - Address formatting rules

### Test Coverage Goals

- Line Coverage: >95%
- Branch Coverage: >90%
- Method Coverage: 100%
- Edge Cases: All documented

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Inspired by functional programming concepts from F# and Rust
- Built with modern C# features
- Designed for real-world applications
