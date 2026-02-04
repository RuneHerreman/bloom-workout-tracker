namespace Bloom.Application.Common;

public readonly struct Unit 
{ 
    public static Unit Value { get; } = new Unit(); 
}

public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T Value { get; private set; } = default!;
    public string[] Errors { get; private set; } = Array.Empty<string>();

    protected Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    protected Result(string[] errors)
    {
        IsSuccess = false;
        Errors = errors;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(params string[] errors) => new(errors);
}

// Non-generic version
public class Result : Result<Unit>
{
    public Result(bool isSuccess) : base(isSuccess ? Unit.Value : default!) { }
    public Result(string[] errors) : base(errors) { }

    public static Result Success() => new(true);
    public static Result Failure(params string[] errors) => new(errors);
}