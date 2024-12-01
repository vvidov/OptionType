namespace OptionType;

public class Option<T>
{
    private readonly T? _value;
    private readonly bool _hasValue;

    private Option(T? value, bool hasValue)
    {
        _value = value;
        _hasValue = hasValue;
    }

    public static Option<T> Some(T value) => 
        new Option<T>(value, true);

    public static Option<T> None() => 
        new Option<T>(default, false);

    public bool IsSome => _hasValue;

    public T Unwrap(Func<T> defaultValueProvider) => 
        _hasValue ? _value! : defaultValueProvider();

    public Option<TResult> Map<TResult>(Func<T, TResult> mapper) =>
        _hasValue ? Option<TResult>.Some(mapper(_value!)) : Option<TResult>.None();

    public Option<TResult> Bind<TResult>(Func<T, Option<TResult>> binder) =>
        _hasValue ? binder(_value!) : Option<TResult>.None();

    public TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none) =>
        _hasValue ? some(_value!) : none();

    public override string ToString() =>
        _hasValue ? $"Some({_value})" : "None";
}
