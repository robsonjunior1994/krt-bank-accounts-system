using KRT.BankAccounts.Api._01_Presentation.Dtos.Response;
using KRT.BankAccounts.Api._01_Presentation.DTOs.Request;
using KRT.BankAccounts.Api._01_Presentation.DTOs.Response;
using KRT.BankAccounts.Api._01_Presentation.Helpers;
using KRT.BankAccounts.Api._02_Application.Interfaces.Services;
using KRT.BankAccounts.Api._02_Application.Shared;
using Microsoft.AspNetCore.Mvc;

namespace KRT.BankAccounts.Api._01_Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAccountRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ",
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                );

                var response = ResponseDto.Failure(
                    errors,
                    StatusCodes.Status400BadRequest.ToString(),
                    "Erro de validação nos dados enviados."
                );

                return StatusCode(StatusCodes.Status400BadRequest, response);
            }

            var result = await _accountService.CreateAsync(request);

            if (!result.IsSuccess)
            {
                int statusCode = ErrorMapper.MapErrorToStatusCode(result.ErrorCode);
                var response = ResponseDto.Failure(
                    result.ErrorMessage,
                    statusCode.ToString(),
                    result.ErrorMessage);

                return StatusCode(statusCode, response);
            }

            return HandleResult(result, "Conta criada com sucesso.", StatusCodes.Status201Created);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _accountService.GetByIdAsync(id);

            if (!result.IsSuccess)
            {
                int statusCode = ErrorMapper.MapErrorToStatusCode(result.ErrorCode);
                var response = ResponseDto.Failure(
                    result.ErrorMessage,
                    statusCode.ToString(),
                    result.ErrorMessage);

                return StatusCode(statusCode, response);
            }

            return HandleResult(result, "Conta encontrada com sucesso.", StatusCodes.Status200OK);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _accountService.GetAllAsync(pageNumber, pageSize);

            if (!result.IsSuccess)
            {
                int statusCode = ErrorMapper.MapErrorToStatusCode(result.ErrorCode);
                var response = ResponseDto.Failure(
                    result.ErrorMessage,
                    statusCode.ToString(),
                    result.ErrorMessage);

                return StatusCode(statusCode, response);
            }

            return HandleResult(result, "Contas listadas com sucesso.", StatusCodes.Status200OK);
        }


        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] bool activate)
        {
            var result = await _accountService.UpdateStatusAsync(id, activate);

            if (!result.IsSuccess)
            {
                int statusCode = ErrorMapper.MapErrorToStatusCode(result.ErrorCode);
                var response = ResponseDto.Failure(
                    result.ErrorMessage,
                    statusCode.ToString(),
                    result.ErrorMessage);

                return StatusCode(statusCode, response);
            }

            return HandleResult(result, "Status da conta atualizado com sucesso.", StatusCodes.Status200OK);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _accountService.DeleteAsync(id);

            if (!result.IsSuccess)
            {
                int statusCode = ErrorMapper.MapErrorToStatusCode(result.ErrorCode);
                var response = ResponseDto.Failure(
                    result.ErrorMessage,
                    statusCode.ToString(),
                    result.ErrorMessage);

                return StatusCode(statusCode, response);
            }

            return HandleResult(result, "Conta excluída com sucesso.", StatusCodes.Status200OK);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAccountRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                var result = Result<AccountResponse>.Failure(errors, ErrorCode.VALIDATION_ERROR);
                return HandleResult(result, "Erro de validação nos dados enviados.", StatusCodes.Status400BadRequest);
            }

            var resultUpdate = await _accountService.UpdateAsync(id, request);

            if (!resultUpdate.IsSuccess)
            {
                int statusCode = ErrorMapper.MapErrorToStatusCode(resultUpdate.ErrorCode);
                var response = ResponseDto.Failure(resultUpdate.ErrorMessage, statusCode.ToString(), resultUpdate.ErrorMessage);
                return StatusCode(statusCode, response);
            }

            return HandleResult(resultUpdate, "Conta atualizada com sucesso.", StatusCodes.Status200OK);
        }

        private IActionResult HandleResult<T>(Result<T> result, string successMessage, int successStatusCode)
        {
            if (result.IsSuccess)
            {
                var successResponse = ResponseDto.Success(
                    successMessage,
                    successStatusCode.ToString(),
                    result.Data);

                return StatusCode(successStatusCode, successResponse);
            }

            int statusCode = ErrorMapper.MapErrorToStatusCode(result.ErrorCode);
            var response = ResponseDto.Failure(result.ErrorMessage, statusCode.ToString(), result.ErrorMessage);

            return StatusCode(statusCode, response);
        }

        private IActionResult HandleResult(Result result, string successMessage, int successStatusCode)
        {
            if (result.IsSuccess)
            {
                var successResponse = ResponseDto.Success(
                    successMessage,
                    successStatusCode.ToString());

                return StatusCode(successStatusCode, successResponse);
            }

            int statusCode = ErrorMapper.MapErrorToStatusCode(result.ErrorCode);
            var response = ResponseDto.Failure(result.ErrorMessage, statusCode.ToString(), result.ErrorMessage);

            return StatusCode(statusCode, response);
        }


    }
}
