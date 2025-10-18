using System.Diagnostics.CodeAnalysis;

namespace KRT.BankAccounts.Api._01_Presentation.Dtos.Response
{
    [ExcludeFromCodeCoverage]
    public sealed class ResponseDto
    {
        public bool IsSuccess { get; init; }
        public string Message { get; init; }
        public string StatusCode { get; init; }
        public object? Data { get; init; }
        public object? Errors { get; init; }

        private ResponseDto(bool isSuccess, string message, string statusCode, object? data, object? errors)
        {
            IsSuccess = isSuccess;
            Message = message;
            StatusCode = statusCode;
            Data = data;
            Errors = errors;
        }

        public static ResponseDto Success(string message, string statusCode = "200", object? data = null) =>
            new ResponseDto(true, message, statusCode, data, null);

        public static ResponseDto Failure(string message, string statusCode = "400", object? errors = null) =>
            new ResponseDto(false, message, statusCode, null, errors);
    }

}
