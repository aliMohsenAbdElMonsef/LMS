namespace LMS.MVC.Services.Contracts.Middleware
{
    public class AuthRedirectMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthRedirectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.ToString().ToLower();
            var isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false;

            var allowPaths = new[]
            {
                "/account/login",
                "/account/signup",
                "/account/forgotpassword",
                "/account/resetpassword",
                "/css",
                "/js",
                "/images",
                "/lib"
            };

            bool isAllowedPath = allowPaths.Any(p => path.StartsWith(p));

            if (!isAuthenticated && !isAllowedPath)
            {
                if (context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.Redirect("/Account/Login");
                    return;
                }

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            await _next(context);
        }

    }

}