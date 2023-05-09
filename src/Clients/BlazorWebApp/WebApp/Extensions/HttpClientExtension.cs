using System.Net.Http.Json;

namespace WebApp.Extensions
{
    public static class HttpClientExtension
    {

        public static async Task<TResult> PostWithGettingResponseAsync<TResult, TValue>(this HttpClient httpClient, string url, TValue body)
        {

            var httpResponse = await httpClient.PostAsJsonAsync(url, body);

            if (httpResponse.IsSuccessStatusCode)
            {
                return await httpResponse.Content.ReadFromJsonAsync<TResult>();
            }

            return default;

        }

        public static async Task PostOnlyAsync<TValue>(this HttpClient httpClient, string url, TValue body)
        {
            await httpClient.PostAsJsonAsync(url, body);
        }

        public static async Task<TResult> GetAsync<TResult>(this HttpClient httpClient, string url)
        {
            return await httpClient.GetFromJsonAsync<TResult>(url);
        }
    }
}
