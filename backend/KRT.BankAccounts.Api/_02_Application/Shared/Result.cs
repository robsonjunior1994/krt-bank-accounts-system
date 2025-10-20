namespace KRT.BankAccounts.Api._02_Application.Shared;

public class Result
{
    public bool IsSuccess { get; }
    public string ErrorMessage { get; }
    public string ErrorCode { get; }

    protected Result(bool isSuccess, string errorMessage, string errorCode)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }

    public static Result Success() => new Result(true, null, null);
    public static Result Failure(string message, string errorCode) =>
        new Result(false, message, errorCode);
}

public class Result<T> : Result
{
    public T Data { get; }

    private Result(bool isSuccess, T data, string errorMessage, string errorCode)
        : base(isSuccess, errorMessage, errorCode)
    {
        Data = data;
    }

    public static Result<T> Success(T data) =>
        new Result<T>(true, data, null, null);

    public static new Result<T> Failure(string message, string errorCode) =>
        new Result<T>(false, default, message, errorCode);
}
