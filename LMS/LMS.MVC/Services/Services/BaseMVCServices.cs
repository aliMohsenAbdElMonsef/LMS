using LMS.MVC.Services.Contracts;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace LMS.MVC.Services.Services
{
    internal abstract class BaseMVCServices : IBaseMVCServices
    {
        protected readonly HttpClient _client;
        private readonly IHttpContextAccessor _contextAccessor;

        protected BaseMVCServices(HttpClient client, IHttpContextAccessor contextAccessor)
        {
            _client = client;
            _contextAccessor = contextAccessor;
        }

        protected async Task<T> SendRequestAsync<T>(Func<Task<HttpResponseMessage>> sendRequest)
        {
            await AttachAccessTokenAsync();

            var response = await sendRequest();

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                if (await TryRefreshTokenAsync())
                {
                    await AttachAccessTokenAsync();
                    response = await sendRequest(); 
                }
            }

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API Error: {content}");

            if (string.IsNullOrWhiteSpace(content))
                throw new Exception("API returned empty response");

            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        private async Task AttachAccessTokenAsync()
        {
            var context = _contextAccessor.HttpContext;
            var token = context?.Request.Cookies["AccessToken"];

            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _client.DefaultRequestHeaders.Authorization = null;
            }
        }

        private async Task<bool> TryRefreshTokenAsync()
        {
            var context = _contextAccessor.HttpContext;
            var refreshToken = context?.Request.Cookies["RefreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return false;

            var request = new HttpRequestMessage(HttpMethod.Post, "api/user/refresh-token");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"refreshToken", refreshToken}
            });

            var response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return false;

            var json = await response.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<dynamic>(json);

            context!.Response.Cookies.Append("AccessToken", (string)tokenData!.accessToken);
            return true;
        }

        public Task<T> GetAsync<T>(string url) =>
            SendRequestAsync<T>(() => _client.GetAsync(url));

        public Task<T> PostAsync<T>(string url, HttpContent content) =>
            SendRequestAsync<T>(() => _client.PostAsync(url, content));

        public Task<T> DeleteAsync<T>(string url) =>
            SendRequestAsync<T>(() => _client.DeleteAsync(url));

        public Task<T> PutAsync<T>(string url, HttpContent content) =>
            SendRequestAsync<T>(() => _client.PutAsync(url, content));
    }
}
