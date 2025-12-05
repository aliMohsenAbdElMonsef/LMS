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
            try
            {
                var token = await _tokenService.GetAccessTokenAsync();

                if (!string.IsNullOrEmpty(token))
                {

                    if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        token = token.Substring(7);
                    }

                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch (Exception)
            {

            }

            var response = await base.SendAsync(request, cancellationToken);


            return response;
        }

    }
}