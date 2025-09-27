using FileCloud.Desktop.Models.Responses;

namespace FileCloud.Desktop.Services.Configurations
{
    public interface ILoginService
    {
        Task DeleteAccount();
        Task<AuthResponse> Login(string login, string password);
        Task Logout();
        Task<AuthResponse> Register(string login, string password, string email);
    }
}