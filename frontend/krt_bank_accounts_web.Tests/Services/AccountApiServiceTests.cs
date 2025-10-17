using System.Net;
using System.Text.Json;
using krt_bank_accounts_web.Models;
using krt_bank_accounts_web.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace krt_bank_accounts_web.Tests.Services
{
    public class AccountApiServiceTests
    {
        private readonly Mock<ILogger<AccountApiService>> _loggerMock;

        public AccountApiServiceTests()
        {
            _loggerMock = new Mock<ILogger<AccountApiService>>();
        }

        // cria HttpClient com resposta mockada
        private HttpClient CreateMockHttpClient<T>(T responseObj, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var handler = new MockHttpMessageHandler(JsonSerializer.Serialize(responseObj), statusCode);
            return new HttpClient(handler) { BaseAddress = new Uri("https://localhost:7020/api/") };
        }

        [Fact]
        public async Task GetAllAsync_DeveRetornarListaDeContas_QuandoApiResponderComSucesso()
        {
            // Arrange
            var mockResponse = new ApiResponse<PagedResponse<AccountViewModel>>
            {
                IsSuccess = true,
                Message = "Contas listadas com sucesso.",
                StatusCode = "200",
                Data = new PagedResponse<AccountViewModel>
                {
                    Items = new List<AccountViewModel>
                    {
                        new() { Id = 1, Name = "Robson", Cpf = "12345678900", Status = "Active" }
                    },
                    TotalCount = 1,
                    CurrentPage = 1,
                    PageSize = 5,
                    TotalPages = 1
                }
            };

            var httpClient = CreateMockHttpClient(mockResponse);
            var service = new AccountApiService(httpClient, _loggerMock.Object);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal("Robson", result.Items.First().Name);
        }

        [Fact]
        public async Task GetAllAsync_DeveRetornarNull_QuandoApiOffline()
        {
            // Arrange
            var handler = new ThrowingHttpMessageHandler(new HttpRequestException("API offline"));
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://localhost:7020/api/") };
            var service = new AccountApiService(httpClient, _loggerMock.Object);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_DeveRetornarContaCriada_QuandoSucesso()
        {
            // Arrange
            var mockResponse = new ApiResponse<AccountViewModel>
            {
                IsSuccess = true,
                Message = "Conta criada com sucesso.",
                StatusCode = "201",
                Data = new AccountViewModel
                {
                    Id = 99,
                    Name = "Cliente Teste",
                    Cpf = "11122233344",
                    Status = "Active"
                }
            };

            var httpClient = CreateMockHttpClient(mockResponse, HttpStatusCode.Created);
            var service = new AccountApiService(httpClient, _loggerMock.Object);

            // Act
            var result = await service.CreateAsync(new AccountViewModel { Name = "Cliente Teste", Cpf = "11122233344" });

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal("Cliente Teste", result.Data.Name);
        }
    }

    // Mock básico de HttpMessageHandler
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _responseContent;
        private readonly HttpStatusCode _statusCode;

        public MockHttpMessageHandler(string responseContent, HttpStatusCode statusCode)
        {
            _responseContent = responseContent;
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage
            {
                StatusCode = _statusCode,
                Content = new StringContent(_responseContent)
            };
            return Task.FromResult(response);
        }
    }

    // Mock que sempre lança exceção (para simular API offline)
    public class ThrowingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Exception _exception;

        public ThrowingHttpMessageHandler(Exception exception)
        {
            _exception = exception;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw _exception;
        }
    }
}
