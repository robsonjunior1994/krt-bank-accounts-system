using System.Net.Http.Json;
using krt_bank_accounts_web.Models;

namespace krt_bank_accounts_web.Services
{
    public class AccountApiService
    {
        private readonly HttpClient _http;

        public AccountApiService(HttpClient http)
        {
            _http = http;
            _http.BaseAddress = new Uri("https://localhost:7020/api/");
        }

        public async Task<PagedResponse<AccountViewModel>?> GetAllAsync(int pageNumber = 1, int pageSize = 5)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<PagedResponse<AccountViewModel>>>(
                $"account?pageNumber={pageNumber}&pageSize={pageSize}");

            return response?.Data;
        }

        public async Task<ApiResponse<AccountViewModel>?> CreateAsync(AccountViewModel account)
        {
            var response = await _http.PostAsJsonAsync("account", new
            {
                name = account.Name,
                cpf = account.Cpf
            });

            return await response.Content.ReadFromJsonAsync<ApiResponse<AccountViewModel>>();
        }

        public async Task<AccountViewModel?> GetByIdAsync(int id)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<AccountViewModel>>($"account/{id}");
            return response?.Data;
        }

        public async Task<ApiResponse<AccountViewModel>?> UpdateAsync(int id, AccountViewModel account)
        {
            var response = await _http.PutAsJsonAsync($"account/{id}", new
            {
                name = account.Name,
                cpf = account.Cpf
            });

            return await response.Content.ReadFromJsonAsync<ApiResponse<AccountViewModel>>();
        }

        public async Task<ApiResponse<object>?> DeleteAsync(int id, AccountViewModel account)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"account/{id}")
            {
                Content = JsonContent.Create(new
                {
                    name = account.Name,
                    cpf = account.Cpf
                })
            };

            var response = await _http.SendAsync(request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        }

        public async Task<ApiResponse<AccountViewModel>?> UpdateStatusAsync(int id, bool activate)
        {
            var response = await _http.PutAsync($"account/{id}/status?activate={activate}", null);
            return await response.Content.ReadFromJsonAsync<ApiResponse<AccountViewModel>>();
        }

    }
}
