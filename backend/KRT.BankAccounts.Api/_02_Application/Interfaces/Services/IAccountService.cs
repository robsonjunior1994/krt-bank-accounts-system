using KRT.BankAccounts.Api._01_Presentation.DTOs.Request;
using KRT.BankAccounts.Api._01_Presentation.DTOs.Response;
using KRT.BankAccounts.Api._02_Application.Shared;

namespace KRT.BankAccounts.Api._02_Application.Interfaces.Services
{
    public interface IAccountService
    {
        Task<Result<AccountResponse>> CreateAsync(CreateAccountRequest request);
        Task<Result<AccountResponse>> GetByIdAsync(int id);
        Task<Result<PagedResult<AccountResponse>>> GetAllAsync(int pageNumber, int pageSize);
        Task<Result> DeleteAsync(int id);
        Task<Result<AccountResponse>> UpdateStatusAsync(int id, bool ativar);

    }
}
