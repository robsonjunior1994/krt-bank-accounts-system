using KRT.BankAccounts.Api._01_Presentation.DTOs.Request;
using KRT.BankAccounts.Api._01_Presentation.DTOs.Response;
using KRT.BankAccounts.Api._01_Presentation.Helpers;
using KRT.BankAccounts.Api._02_Application.Interfaces.Infra;
using KRT.BankAccounts.Api._02_Application.Interfaces.Services;
using KRT.BankAccounts.Api._02_Application.Shared;
using KRT.BankAccounts.Api._03_Domain.Entities;
using KRT.BankAccounts.Api._03_Domain.Enums;
using KRT.BankAccounts.Api._03_Domain.Interfaces;

namespace KRT.BankAccounts.Api._02_Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _repository;
        private readonly IMessagePublisher _publisher;
        private readonly ICacheService _cache;

        public AccountService(IAccountRepository repository, IMessagePublisher publisher, ICacheService cache)
        {
            _repository = repository;
            _publisher = publisher;
            _cache = cache;
        }

        public async Task<Result<AccountResponse>> CreateAsync(CreateAccountRequest request)
        {
            try
            {
                if (await _repository.ExistsByCpfAsync(request.Cpf))
                    return Result<AccountResponse>.Failure(
                        "CPF já cadastrado.",
                        ErrorCode.RESOURCE_ALREADY_EXISTS
                    );

                if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3)
                    return Result<AccountResponse>.Failure(
                        "O nome deve conter ao menos 3 caracteres.",
                        ErrorCode.VALIDATION_ERROR
                    );

                var account = new Account(request.Name, request.Cpf);

                await _repository.AddAsync(account);

                // Publica evento (para outras áreas, ex: cartões, fraude, etc)
                //await _publisher.PublishAsync("account.created", new
                //{
                //    account.Id,
                //    account.Name,
                //    account.Cpf,
                //    account.Status
                //});

                // Limpa cache (opcional, se usar caching de contas)
                //await _cache.RemoveAsync($"account_{account.Id}");

                var response = new AccountResponse(account);
                return Result<AccountResponse>.Success(response);
            }
            catch (Exception)
            {
                return Result<AccountResponse>.Failure(
                    "Ocorreu um erro ao criar a conta.",
                    ErrorCode.DATABASE_ERROR
                );
            }
        }

        public async Task<Result> DeleteAsync(int id)
        {
            try
            {
                var account = await _repository.GetByIdAsync(id);
                if (account == null)
                    return Result.Failure("Conta não encontrada.", ErrorCode.NOT_FOUND);

                await _repository.DeleteAsync(account);

                // Publicação de evento (RabbitMQ)
                //await _publisher.PublishAsync("account.deleted", new
                //{
                //    account.Id,
                //    account.Name,
                //    account.Cpf,
                //    Status = "Deletada"
                //});

                // Limpeza de cache (caso exista)
                //await _cache.RemoveAsync($"account_{account.Id}");

                return Result.Success();
            }
            catch (Exception)
            {
                return Result.Failure(
                    "Ocorreu um erro ao excluir a conta.",
                    ErrorCode.DATABASE_ERROR
                );
            }
        }



        public async Task<Result<PagedResult<AccountResponse>>> GetAllAsync(int pageNumber, int pageSize)
        {
            try
            {
                // Tenta buscar no cache primeiro
                //var cacheKey = "accounts_all";
                //var cachedAccounts = await _cache.GetAsync<IEnumerable<AccountResponse>>(cacheKey);

                //if (cachedAccounts != null && cachedAccounts.Any())
                //    return Result<IEnumerable<AccountResponse>>.Success(cachedAccounts);
                var totalCount = await _repository.CountAsync();
                if (totalCount == 0)
                    return Result<PagedResult<AccountResponse>>.Failure(
                        "Nenhuma conta encontrada.",
                        ErrorCode.NOT_FOUND
                    );

                var accounts = await _repository.GetPagedAsync(pageNumber, pageSize);

                var responseList = accounts.Select(a => new AccountResponse(a)).ToList();

                // Armazena no cache (para evitar custo de consulta)
                //await _cache.SetAsync(cacheKey, responseList, TimeSpan.FromHours(1));

                var pagedResult = new PagedResult<AccountResponse>(responseList, totalCount, pageNumber, pageSize);

                return Result<PagedResult<AccountResponse>>.Success(pagedResult);
            }
            catch (Exception)
            {
                return Result<PagedResult<AccountResponse>>.Failure(
                    "Ocorreu um erro ao buscar as contas.",
                    ErrorCode.DATABASE_ERROR
                );
            }
        }


        public async Task<Result<AccountResponse>> GetByIdAsync(int id)
        {
            try
            {
                // Tenta buscar do cache primeiro
                //var cacheKey = $"account_{id}";
                //var cachedAccount = await _cache.GetAsync<AccountResponse>(cacheKey);

                //if (cachedAccount != null)
                //    return Result<AccountResponse>.Success(cachedAccount);

                var account = await _repository.GetByIdAsync(id);
                if (account == null)
                    return Result<AccountResponse>.Failure(
                        "Conta não encontrada.",
                        ErrorCode.NOT_FOUND
                    );

                var response = new AccountResponse(account);

                // Armazena no cache (para consultas futuras)
                //await _cache.SetAsync(cacheKey, response, TimeSpan.FromHours(1));

                return Result<AccountResponse>.Success(response);
            }
            catch (Exception)
            {
                return Result<AccountResponse>.Failure(
                    "Ocorreu um erro ao buscar a conta.",
                    ErrorCode.DATABASE_ERROR
                );
            }
        }


        public async Task<Result<AccountResponse>> UpdateStatusAsync(int id, bool ativar)
        {
            try
            {
                var account = await _repository.GetByIdAsync(id);
                if (account == null)
                    return Result<AccountResponse>.Failure("Conta não encontrada.", ErrorCode.NOT_FOUND);

                try
                {
                    if (ativar)
                        account.Activate();
                    else
                        account.Deactivate();
                }
                catch (InvalidOperationException ex)
                {
                    return Result<AccountResponse>.Failure(ex.Message, ErrorCode.VALIDATION_ERROR);
                }

                await _repository.UpdateAsync(account);

                //// Publica evento conforme a ação
                //var eventName = ativar ? "account.activated" : "account.deactivated";
                //await _publisher.PublishAsync(eventName, new
                //{
                //    account.Id,
                //    account.Name,
                //    account.Cpf,
                //    Status = account.Status.ToString()
                //});

                // Limpa cache
                //await _cache.RemoveAsync($"account_{account.Id}");
                //await _cache.RemoveAsync("accounts_all");

                var response = new AccountResponse(account);
                return Result<AccountResponse>.Success(response);
            }
            catch (Exception)
            {
                return Result<AccountResponse>.Failure(
                    "Ocorreu um erro ao atualizar o status da conta.",
                    ErrorCode.DATABASE_ERROR
                );
            }
        }
    }
}
