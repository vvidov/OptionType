namespace Result;

public class Result<T>
{
    private readonly T? _value;
    private readonly string? _error;

    private Result(T value)
    {
        _value = value;
        _error = null;
        IsSuccess = true;
    }

    private Result(string error)
    {
        _value = default;
        _error = error;
        IsSuccess = false;
    }

    public bool IsSuccess { get; }
    public bool IsError => !IsSuccess;

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Error(string error) => new(error);

    public string? GetError() => _error;

    public T Unwrap(T defaultValue) => IsSuccess ? _value! : defaultValue;

    public T Unwrap(Func<T> defaultValueProvider) => 
        IsSuccess ? _value! : defaultValueProvider();

    public Result<TResult> OnSuccess<TResult>(Func<T, TResult> mapper) =>
        IsSuccess 
            ? Result<TResult>.Success(mapper(_value!))
            : Result<TResult>.Error(_error!);

    public TResult Match<TResult>(Func<T, TResult> success, Func<string, TResult> failure) =>
        IsSuccess ? success(_value!) : failure(_error!);
}
