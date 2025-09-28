using FileCloud.Desktop.Models;
using FileCloud.Desktop.Models.Requests;
using FileCloud.Desktop.Models.Responses;
using FileCloud.Desktop.Services.Configurations;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace FileCloud.Desktop.Services.Services
{
    public class LoginService : ILoginService
    {
        private readonly string _apiSubUrl = "/api/Auth";
        private readonly HttpClient _client;
        private readonly ILogger<LoginService> _logger;

        public LoginService(IAppSettingsService settings, ILogger<LoginService> logger)
        {
            _logger = logger;
            _client = new HttpClient
            {
                BaseAddress = new Uri(settings.ApiBaseUrl)
            };
        }

        /// <summary>
        /// Установка JWT токена
        /// </summary>
        /// <param name="token">JWT token</param>
        public void SetToken(string token)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);
        }

        public async Task<AuthResponse> Register(string login, string password, string email)
        {
            return await ServerStateService.ExecuteIfServerActive<AuthResponse>(_logger, async () =>
            {
                var registerRequest = new RegisterRequest(login, password, email);
                var response = await _client.PostAsJsonAsync($"{_apiSubUrl}/register", registerRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError(error);
                    throw new HttpRequestException($"Ошибка при регистрации: {error}");
                }

                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

                return authResponse ?? throw new InvalidOperationException("Invalid response format");
            });
        }

        public async Task<AuthResponse> Login(string login, string password)
        {
            return await ServerStateService.ExecuteIfServerActive<AuthResponse>(_logger, async () =>
            {
                var loginRequest = new LoginRequest(login, password);
                var response = await _client.PostAsJsonAsync($"{_apiSubUrl}/login", loginRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError(error);
                    throw new HttpRequestException($"Ошибка при входе: {error}");
                }

                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

                return authResponse ?? throw new InvalidOperationException("Invalid response format");
            });
        }

        public Task Logout()
        {
            throw new NotImplementedException();
        }

        public Task DeleteAccount()
        {
            throw new NotImplementedException();
        }
    }
}
