using krt_bank_accounts_web.Models;
using krt_bank_accounts_web.Services.Interfaces;

namespace krt_bank_accounts_web.Services
{
    public class AccountApiService : IAccountApiService
    {
        private readonly HttpClient _http;
        private readonly ILogger<AccountApiService> _logger;

        public AccountApiService(HttpClient http, ILogger<AccountApiService> logger)
        {
            _http = http;
            _logger = logger;
            _http.BaseAddress = new Uri("https://localhost:7020/api/");
        }

        private ApiResponse<T> ErrorResponse<T>(string message)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Message = message,
                StatusCode = "500",
                Data = default
            };
        }

        public async Task<PagedResponse<AccountViewModel>?> GetAllAsync(int pageNumber = 1, int pageSize = 5)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<ApiResponse<PagedResponse<AccountViewModel>>>(
                    $"account?pageNumber={pageNumber}&pageSize={pageSize}");

                return response?.Data;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Falha de conexão com a API.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao acessar a API.");
                return null;
            }
        }

        public async Task<ApiResponse<AccountViewModel>?> CreateAsync(AccountViewModel account)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("account", new
                {
                    name = account.Name,
                    cpf = account.Cpf
                });

                return await response.Content.ReadFromJsonAsync<ApiResponse<AccountViewModel>>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar conta.");
                return ErrorResponse<AccountViewModel>("Erro ao conectar à API (CreateAsync).");
            }
        }

        public async Task<AccountViewModel?> GetByIdAsync(int id)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<ApiResponse<AccountViewModel>>($"account/{id}");
                return response?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter conta {Id}.", id);
                return null;
            }
        }

        public async Task<ApiResponse<AccountViewModel>?> UpdateAsync(int id, AccountViewModel account)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"account/{id}", new
                {
                    name = account.Name,
                    cpf = account.Cpf
                });

                return await response.Content.ReadFromJsonAsync<ApiResponse<AccountViewModel>>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar conta {Id}.", id);
                return ErrorResponse<AccountViewModel>("Erro ao conectar à API (UpdateAsync).");
            }
        }

        public async Task<ApiResponse<object>?> DeleteAsync(int id, AccountViewModel account)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir conta {Id}.", id);
                return ErrorResponse<object>("Erro ao conectar à API (DeleteAsync).");
            }
        }

        public async Task<ApiResponse<AccountViewModel>?> UpdateStatusAsync(int id, bool activate)
        {
            try
            {
                var response = await _http.PutAsync($"account/{id}/status?activate={activate}", null);
                return await response.Content.ReadFromJsonAsync<ApiResponse<AccountViewModel>>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar status da conta {Id}.", id);
                return ErrorResponse<AccountViewModel>("Erro ao conectar à API (UpdateStatusAsync).");
            }
        }

    }
}
