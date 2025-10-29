using LMS.MVC.Services.Contracts.Services;
using System.Net.Http.Headers;

namespace LMS.MVC.Services.Handlers
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly ITokenService _tokenService;

        public AuthHeaderHandler(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"[AuthHeaderHandler] Starting request to: {request.RequestUri}");

            try
            {
                Console.WriteLine($"[AuthHeaderHandler] Calling TokenService.GetAccessTokenAsync()...");
                var token = await _tokenService.GetAccessTokenAsync();
                Console.WriteLine($"[AuthHeaderHandler] TokenService returned: {(string.IsNullOrEmpty(token) ? "NULL" : $"TOKEN (length: {token.Length})")}");

                if (!string.IsNullOrEmpty(token))
                {
                    // Validate the token format
                    if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        token = token.Substring(7); // Remove "Bearer " prefix if present
                        Console.WriteLine($"[AuthHeaderHandler] Removed 'Bearer ' prefix");
                    }

                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    Console.WriteLine($"[AuthHeaderHandler] ✅ Token attached to request headers");
                    Console.WriteLine($"[AuthHeaderHandler] Authorization header set: {request.Headers.Authorization != null}");
                }
                else
                {
                    Console.WriteLine("[AuthHeaderHandler] ❌ No token found!");

                    // Check if we're making a request to a protected endpoint
                    if (request.RequestUri.ToString().Contains("/api/User/all"))
                    {
                        Console.WriteLine("[AuthHeaderHandler] ⚠️  This is a protected endpoint but no token is available!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthHeaderHandler] ❌ Error getting token: {ex.Message}");
                Console.WriteLine($"[AuthHeaderHandler] Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine($"[AuthHeaderHandler] Sending request to: {request.RequestUri}");

            var response = await base.SendAsync(request, cancellationToken);

            Console.WriteLine($"[AuthHeaderHandler] Response status: {response.StatusCode}");
            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[AuthHeaderHandler] Error response: {responseContent}");
            }

            return response;
        }

    }
}