namespace krt_bank_accounts_web.Models
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public T? Data { get; set; }
        public object? Errors { get; set; }
    }
}
