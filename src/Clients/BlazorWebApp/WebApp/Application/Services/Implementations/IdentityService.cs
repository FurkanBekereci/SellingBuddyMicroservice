using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using WebApp.Application.Services.Interfaces;
using WebApp.Domain.Models.User;
using WebApp.Extensions;
using WebApp.Utils;

namespace WebApp.Application.Services.Implementations
{
    public class IdentityService : IIdentityService
    {
        private readonly HttpClient _httpClient;
        private readonly ISyncLocalStorageService _syncLocalStorageService;
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public IdentityService(HttpClient httpClient, ISyncLocalStorageService syncLocalStorageService, AuthenticationStateProvider authenticationStateProvider)
        {
            _httpClient = httpClient;
            _syncLocalStorageService = syncLocalStorageService;
            _authenticationStateProvider = authenticationStateProvider;
        }

        public bool IsLoggedIn => !string.IsNullOrEmpty(GetUserToken());

        public string GetUserName()
        {
            return _syncLocalStorageService.GetUserName();
        }

        public string GetUserToken()
        {
            return _syncLocalStorageService.GetToken();
        }

        public async Task<bool> Login(UserLoginRequest userLoginRequest)
        {
            var response = await _httpClient.PostWithGettingResponseAsync<UserLoginResponse, UserLoginRequest>("auth/login", userLoginRequest);

            if (!string.IsNullOrEmpty(response.UserToken))
            {
                _syncLocalStorageService.SetToken(response.UserToken);
                _syncLocalStorageService.SetUserName(response.UserName);

                ((AuthStateProvider)_authenticationStateProvider).NotifyWhenUserLoggedIn(response.UserName);

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", response.UserToken);

                return true;
            }

            return false;
        }

        public void Logout()
        {
            _syncLocalStorageService.RemoveToken();
            _syncLocalStorageService.RemoveUserName();

            ((AuthStateProvider)_authenticationStateProvider).NotifyWhenUserLoggedOut();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
