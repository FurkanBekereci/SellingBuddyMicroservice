using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Security.Claims;
using WebApp.Extensions;
using WebApp.Infrastructure;

namespace WebApp.Utils
{
    public class AuthStateProvider : AuthenticationStateProvider
    {

        private readonly ILocalStorageService _localStorageService;
        private readonly HttpClient _httpClient;
        private readonly AuthenticationState _anonymous;
        private readonly AppStateManager _appState;

        public AuthStateProvider(ILocalStorageService localStorageService, HttpClient httpClient, AppStateManager appState)
        {
            _localStorageService = localStorageService;
            _httpClient = httpClient;
            _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            _appState = appState;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string token = await _localStorageService.GetTokenAsync();

            if (string.IsNullOrEmpty(token)) return _anonymous;

            string userName = await _localStorageService.GetUserNameAsync();

            if (string.IsNullOrEmpty(userName)) return _anonymous;

            var claimsPricipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, userName)
            }, "jwtAuthType"));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return new AuthenticationState(claimsPricipal);
            
        }

        public void NotifyWhenUserLoggedIn(string userName)
        {

            var claimsPricipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, userName)
            }, "jwtAuthType"));

            var authState = Task.FromResult(new AuthenticationState(claimsPricipal));

            NotifyAuthenticationStateChanged(authState);
            _appState.LoginChanged(null);
        }

        public void NotifyWhenUserLoggedOut()
        {
            var authState = Task.FromResult(_anonymous);
            NotifyAuthenticationStateChanged(authState);
            _appState.LoginChanged(null);
        }
    }
}
