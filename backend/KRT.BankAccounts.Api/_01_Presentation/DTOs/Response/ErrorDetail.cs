using System.Diagnostics.CodeAnalysis;

namespace KRT.BankAccounts.Api._01_Presentation.Dtos.Response;

[ExcludeFromCodeCoverage]
public sealed class ErrorDetail
{
    public string Code { get; init; } = default!;
    public string Message { get; init; } = default!;
    public object? Metadata { get; init; }
}
