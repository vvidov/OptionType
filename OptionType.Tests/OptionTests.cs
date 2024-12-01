using Xunit;

namespace OptionType.Tests;

public class OptionTests
{
    [Fact]
    public void Some_CreatesOptionWithValue()
    {
        var option = Option<int>.Some(42);
        Assert.True(option.IsSome);
    }

    [Fact]
    public void None_CreatesEmptyOption()
    {
        var option = Option<int>.None();
        Assert.False(option.IsSome);
    }

    [Fact]
    public void Unwrap_WithSome_ReturnsValue()
    {
        const int expected = 42;
        var option = Option<int>.Some(42);
        var result = option.Unwrap(() => 0);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Unwrap_WithNone_ReturnsDefaultValue()
    {
        const int expected = 42;
        var option = Option<int>.None();
        var result = option.Unwrap(() => expected);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Unwrap_WithNone_UsesLazyEvaluation()
    {
        var option = Option<int>.None();
        var evaluationCount = 0;
        
        // First call should evaluate the default
        var result1 = option.Unwrap(() => { evaluationCount++; return 0; });
        Assert.Equal(1, evaluationCount);
        
        // Second call should evaluate again (proving lazy evaluation)
        var result2 = option.Unwrap(() => { evaluationCount++; return 0; });
        Assert.Equal(2, evaluationCount);
    }

    [Fact]
    public void Map_WithSome_TransformsValue()
    {
        var option = Option<int>.Some(42);
        var result = option.Map(x => x * 2);
        Assert.Equal(84, result.Unwrap(() => 0));
    }

    [Fact]
    public void Map_WithNone_ReturnsNone()
    {
        var option = Option<int>.None();
        var result = option.Map(x => x * 2);
        Assert.False(result.IsSome);
    }

    [Fact]
    public void Bind_WithSome_TransformsToNewOption()
    {
        const int expected = 42;
        var option = Option<int>.Some(expected);
        var result = option.Bind(x => Option<string>.Some(x.ToString()));
        Assert.Equal(expected.ToString(), result.Unwrap(() => ""));
    }

    [Fact]
    public void Bind_WithNone_ReturnsNone()
    {
        var option = Option<int>.None();
        var result = option.Bind(x => Option<string>.Some(x.ToString()));
        Assert.False(result.IsSome);
    }

    [Fact]
    public void Match_WithSome_CallsSomeFunc()
    {
        const int expected = 42;
        var option = Option<int>.Some(expected);
        var result = option.Match(
            some: x => x * 2,
            none: () => 0
        );
        Assert.Equal(expected * 2, result);
    }

    [Fact]
    public void Match_WithNone_CallsNoneFunc()
    {
        var option = Option<int>.None();
        var result = option.Match(
            some: x => x * 2,
            none: () => 0
        );
        Assert.Equal(0, result);
    }

    [Fact]
    public void ToString_WithSome_ShowsValue()
    {
        const int expected = 42;
        var option = Option<int>.Some(expected);
        Assert.Equal($"Some({expected})", option.ToString());
    }

    [Fact]
    public void ToString_WithNone_ShowsNone()
    {
        var option = Option<int>.None();
        Assert.Equal("None", option.ToString());
    }

    [Fact]
    public void Map_WithNone_DoesNotExecuteMapper()
    {
        var mapperExecuted = false;
        var option = Option<int>.None();
        
        option.Map(_ => 
        {
            mapperExecuted = true;
            return 42;
        });
        
        Assert.False(mapperExecuted);
    }

    [Fact]
    public void Bind_WithNone_DoesNotExecuteBinder()
    {
        var binderExecuted = false;
        var option = Option<int>.None();
        
        option.Bind(_ => 
        {
            binderExecuted = true;
            return Option<int>.Some(42);
        });
        
        Assert.False(binderExecuted);
    }

    [Fact]
    public void Match_WithSome_DoesNotExecuteNoneFunc()
    {
        const int expected = 42;
        var noneFuncExecuted = false;
        var option = Option<int>.Some(expected);
        
        option.Match(
            some: x => x,
            none: () => 
            {
                noneFuncExecuted = true;
                return 0;
            }
        );
        
        Assert.False(noneFuncExecuted);
    }

    [Fact]
    public void Match_WithNone_DoesNotExecuteSomeFunc()
    {
        var someFuncExecuted = false;
        var option = Option<int>.None();
        
        option.Match(
            some: x => 
            {
                someFuncExecuted = true;
                return x;
            },
            none: () => 0
        );
        
        Assert.False(someFuncExecuted);
    }

    [Fact]
    public void Unwrap_WithSome_DoesNotExecuteDefaultValueProvider()
    {
        const int expected = 42;
        var defaultProviderExecuted = false;
        var option = Option<int>.Some(expected);
        
        option.Unwrap(() => 
        {
            defaultProviderExecuted = true;
            return 0;
        });
        
        Assert.False(defaultProviderExecuted);
    }

    [Fact]
    public void ChainedOperations_WithSome_ExecutesCorrectly()
    {
        const int expected = 42;
        var option = Option<int>.Some(expected);
        
        var result = option
            .Map(x => x * 2)
            .Bind(x => Option<string>.Some(x.ToString()))
            .Map(s => int.Parse(s))
            .Unwrap(() => 0);
            
        Assert.Equal(expected * 2, result);
    }

    [Fact]
    public void ChainedOperations_WithNone_ShortCircuits()
    {
        var mapperExecuted = false;
        var binderExecuted = false;
        var option = Option<int>.None();
        
        var result = option
            .Map(x => 
            {
                mapperExecuted = true;
                return x * 2;
            })
            .Map(x => x.ToString())
            .Map(s => s + "!")
            .Unwrap(() => "42");
            
        Assert.Equal("42", result);
        Assert.False(mapperExecuted);
        Assert.False(binderExecuted);
    }

    [Fact]
    public void Option_WithReferenceType_HandlesNullValue()
    {
        string? nullString = null;
        var option = Option<string>.Some(nullString!);
        
        Assert.True(option.IsSome);
        Assert.Null(option.Unwrap(() => "default"));
    }
}
