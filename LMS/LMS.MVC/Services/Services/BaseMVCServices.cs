using LMS.MVC.Services.Contracts;
using Newtonsoft.Json;

namespace LMS.MVC.Services.Services
{
    internal abstract class BaseMVCServices : IBaseMVCServices
    {
        protected readonly HttpClient _client;

        protected BaseMVCServices(HttpClient client)
        {
            _client = client;
        }

        public async Task<T> GetAsync<T>(string url)
        {
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new Exception($"API returned null for GET {url}");

            return JsonConvert.DeserializeObject<T>(json);
        }

        public async Task<T> PostAsync<T>(string url, HttpContent content)
        {
            var response = await _client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new Exception($"API returned null for POST {url}");

            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
