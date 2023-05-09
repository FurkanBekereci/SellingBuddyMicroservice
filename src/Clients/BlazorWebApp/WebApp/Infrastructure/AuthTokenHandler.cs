using Blazored.LocalStorage;
using System.Net.Http.Headers;
using WebApp.Extensions;

namespace WebApp.Infrastructure
{
    public class AuthTokenHandler : DelegatingHandler
    {
        private readonly ISyncLocalStorageService _syncLocalStorage;

        public AuthTokenHandler(ISyncLocalStorageService syncLocalStorage)
        {
            _syncLocalStorage = syncLocalStorage;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if(_syncLocalStorage != null)
            {
                var token = _syncLocalStorage.GetToken();
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
