using WebApp.Domain.Models.User;

namespace WebApp.Application.Services.Interfaces
{
    public interface IIdentityService
    {
        string GetUserName();

        string GetUserToken();

        bool IsLoggedIn { get; }

        Task<bool> Login(UserLoginRequest userLoginRequest);

        void Logout();
    }
}
