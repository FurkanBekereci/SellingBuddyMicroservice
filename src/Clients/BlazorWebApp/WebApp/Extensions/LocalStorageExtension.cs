using Blazored.LocalStorage;

namespace WebApp.Extensions
{
    public static class LocalStorageExtension
    {
        private const string TOKEN_KEY = "token";
        private const string USERNAME_KEY = "username";
        public static T GetValue<T>(this ISyncLocalStorageService syncLocalStorageService, string key = null)
        {
            return syncLocalStorageService.GetItem<T>(key);
        }

        public static void SetValue<T>(this ISyncLocalStorageService syncLocalStorageService, T data, string key)
        {
            syncLocalStorageService.SetItem(key, data);
        }

        public static async Task<T> GetValueAsync<T>(this ILocalStorageService localStorageService, string key = null)
        {
            return await localStorageService.GetItemAsync<T>(key);
        }

        public static async Task SetValueAsync<T>(this ILocalStorageService localStorageService, T data, string key)
        {
            await localStorageService.SetItemAsync(key, data);
        }

        public static string GetUserName(this ISyncLocalStorageService syncLocalStorageService) => GetValue<string>(syncLocalStorageService, USERNAME_KEY);
        public static async Task<string> GetUserNameAsync(this ILocalStorageService localStorageService) => await GetValueAsync<string>(localStorageService, USERNAME_KEY);


        public static string GetToken(this ISyncLocalStorageService syncLocalStorageService) => GetValue<string>(syncLocalStorageService, TOKEN_KEY);
        public static async Task<string> GetTokenAsync(this ILocalStorageService localStorageService) => await GetValueAsync<string>(localStorageService, TOKEN_KEY);

        public static void SetUserName(this ISyncLocalStorageService syncLocalStorageService, string userName) => SetValue<string>(syncLocalStorageService, userName, USERNAME_KEY);
        public static async Task SetUserNameAsync(this ILocalStorageService localStorageService, string userName) => await SetValueAsync<string>(localStorageService, userName , USERNAME_KEY);


        public static void SetToken(this ISyncLocalStorageService syncLocalStorageService, string token) => SetValue<string>(syncLocalStorageService, token, TOKEN_KEY);
        public static async Task SetTokenAsync(this ILocalStorageService localStorageService, string token) => await SetValueAsync<string>(localStorageService, token, TOKEN_KEY);

        public static void RemoveUserName(this ISyncLocalStorageService syncLocalStorageService) => syncLocalStorageService.RemoveItem(USERNAME_KEY);
        public static void RemoveUserNameAsync(this ILocalStorageService localStorageService) => localStorageService.RemoveItemAsync(USERNAME_KEY);

        public static void RemoveToken(this ISyncLocalStorageService syncLocalStorageService) => syncLocalStorageService.RemoveItem(TOKEN_KEY);
        public static void RemoveTokenAsync(this ILocalStorageService localStorageService) => localStorageService.RemoveItemAsync(TOKEN_KEY);


    }
}
