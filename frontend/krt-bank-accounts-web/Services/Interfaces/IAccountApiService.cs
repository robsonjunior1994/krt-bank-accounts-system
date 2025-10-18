using krt_bank_accounts_web.Models;

namespace krt_bank_accounts_web.Services.Interfaces
{
    public interface IAccountApiService
    {
        Task<ApiResponse<PagedResponse<AccountViewModel>>?> GetAllAsync(int pageNumber = 1, int pageSize = 5);
        Task<ApiResponse<AccountViewModel>?> CreateAsync(AccountViewModel account);
        Task<AccountViewModel?> GetByIdAsync(int id);
        Task<ApiResponse<AccountViewModel>?> UpdateAsync(int id, AccountViewModel account);
        Task<ApiResponse<object>?> DeleteAsync(int id, AccountViewModel account);
        Task<ApiResponse<AccountViewModel>?> UpdateStatusAsync(int id, bool activate);
    }
}
