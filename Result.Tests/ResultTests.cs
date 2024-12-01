using Xunit;

namespace Result.Tests;

public class ResultTests
{
    [Fact]
    public void Success_CreatesSuccessResult()
    {
        var result = Result<int>.Success(42);
        
        Assert.True(result.IsSuccess);
        Assert.False(result.IsError);
        Assert.Equal(42, result.Unwrap(0));
        Assert.Null(result.GetError());
    }

    [Fact]
    public void Error_CreatesErrorResult()
    {
        var result = Result<int>.Error("Something went wrong");
        
        Assert.False(result.IsSuccess);
        Assert.True(result.IsError);
        Assert.Equal("Something went wrong", result.GetError());
        Assert.Equal(0, result.Unwrap(0));
    }

    [Fact]
    public void OnSuccess_WithSuccess_TransformsValue()
    {
        var result = Result<int>.Success(42)
            .OnSuccess(x => x.ToString());
        
        Assert.True(result.IsSuccess);
        Assert.Equal("42", result.Unwrap(string.Empty));
    }

    [Fact]
    public void OnSuccess_WithError_PropagatesError()
    {
        var result = Result<int>.Error("Something went wrong")
            .OnSuccess(x => x.ToString());
        
        Assert.True(result.IsError);
        Assert.Equal("Something went wrong", result.GetError());
    }

    [Fact]
    public void Match_WithSuccess_CallsSuccessFunc()
    {
        var result = Result<int>.Success(42);
        var matched = result.Match(
            success: value => $"Success: {value}",
            failure: error => $"Error: {error}"
        );
        
        Assert.Equal("Success: 42", matched);
    }

    [Fact]
    public void Match_WithError_CallsErrorFunc()
    {
        var result = Result<int>.Error("Something went wrong");
        var matched = result.Match(
            success: value => $"Success: {value}",
            failure: error => $"Error: {error}"
        );
        
        Assert.Equal("Error: Something went wrong", matched);
    }

    [Fact]
    public void Unwrap_WithSuccess_ReturnsValue()
    {
        var result = Result<int>.Success(42);
        Assert.Equal(42, result.Unwrap(0));
        Assert.Equal(42, result.Unwrap(() => 0));
    }

    [Fact]
    public void Unwrap_WithError_ReturnsDefaultValue()
    {
        var result = Result<int>.Error("Something went wrong");
        Assert.Equal(0, result.Unwrap(0));
        Assert.Equal(-1, result.Unwrap(() => -1));
    }
}
