using KRT.BankAccounts.Api._01_Presentation.DTOs.Request;
using KRT.BankAccounts.Api._01_Presentation.DTOs.Response;
using KRT.BankAccounts.Api._01_Presentation.Helpers;
using KRT.BankAccounts.Api._02_Application.Interfaces.Infra;
using KRT.BankAccounts.Api._02_Application.Interfaces.Services;
using KRT.BankAccounts.Api._02_Application.Shared;
using KRT.BankAccounts.Api._03_Domain.Entities;
using KRT.BankAccounts.Api._03_Domain.Interfaces;
using KRT.BankAccounts.Api._04_Infrastructure.Cache;
using Microsoft.Extensions.Options;

namespace KRT.BankAccounts.Api._02_Application.Services
{

    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _repository;
        private readonly IMessagePublisher _publisher;
        private readonly ICacheService _cache;
        private readonly CacheSettings _settings;

        public AccountService(IAccountRepository repository, IMessagePublisher publisher, ICacheService cache, IOptions<CacheSettings> options)
        {
            _repository = repository;
            _publisher = publisher;
            _cache = cache;
            _settings = options.Value;
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

                await _publisher.PublishAsync("account.created", new
                {
                    account.Id,
                    account.Name,
                    account.Cpf,
                    account.Status,
                    account.CreatedAt,
                    account.UpdatedAt
                });

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

                await _publisher.PublishAsync("account.deleted", new
                {
                    account.Id,
                    account.Name,
                    account.Cpf,
                    Status = "Deletada",
                    account.CreatedAt,
                    account.UpdatedAt
                });

                await _cache.RemoveAsync($"account_{account.Id}");

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
                var totalRecords = await _repository.CountAsync();
                var accounts = await _repository.GetPagedAsync(pageNumber, pageSize)
                                       ?? Enumerable.Empty<Account>();

                var responseList = accounts.Select(a => new AccountResponse(a)).ToList();

                var pagedResult = new PagedResult<AccountResponse>(
                    responseList,
                    totalRecords,
                    pageNumber,
                    pageSize
                );

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
                var cacheKey = $"account_{id}";
                var cachedAccount = await _cache.GetAsync<AccountResponse>(cacheKey);

                if (cachedAccount != null)
                    return Result<AccountResponse>.Success(cachedAccount);

                var account = await _repository.GetByIdAsync(id);
                if (account == null)
                    return Result<AccountResponse>.Failure(
                        "Conta não encontrada.",
                        ErrorCode.NOT_FOUND
                    );

                var response = new AccountResponse(account);

                await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes));

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

                var eventName = ativar ? "account.activate" : "account.disable";
                await _publisher.PublishAsync(eventName, new
                {
                    account.Id,
                    account.Name,
                    account.Cpf,
                    Status = account.Status.ToString(),
                    account.CreatedAt,
                    account.UpdatedAt
                });

                await _cache.RemoveAsync($"account_{account.Id}");

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

        public async Task<Result<AccountResponse>> UpdateAsync(int id, UpdateAccountRequest request)
        {
            try
            {
                var account = await _repository.GetByIdAsync(id);
                if (account == null)
                    return Result<AccountResponse>.Failure("Conta não encontrada.", ErrorCode.NOT_FOUND);

                var exists = await _repository.ExistsByCpfAsync(request.Cpf);
                if (exists && !string.Equals(account.Cpf, request.Cpf, StringComparison.OrdinalIgnoreCase))
                    return Result<AccountResponse>.Failure("CPF já está em uso por outra conta.", ErrorCode.RESOURCE_ALREADY_EXISTS);

                account.Update(request.Name, request.Cpf);

                await _repository.UpdateAsync(account);

                await _cache.RemoveAsync($"account_{account.Id}");

                await _publisher.PublishAsync("account.updated", new
                {
                    account.Id,
                    account.Name,
                    account.Cpf,
                    Status = account.Status.ToString(),
                    account.CreatedAt,
                    account.UpdatedAt
                });

                var response = new AccountResponse(account);
                return Result<AccountResponse>.Success(response);
            }
            catch (Exception)
            {
                return Result<AccountResponse>.Failure(
                    "Ocorreu um erro ao atualizar a conta.",
                    ErrorCode.DATABASE_ERROR
                );
            }
        }

    }
}