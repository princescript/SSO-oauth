namespace AuthService.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var host = context.Request.Host.Host.ToLower();
            var port = context.Request.Host.Port;

            string? tenant = (host, port)
            switch
            {
                ("localhost", 5000) => "erp",
                ("localhost", 5001) => "invex",
                ("localhost", 7227) => "server",

                _ => null
            };

            if (tenant == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Unknown tenant");
                return;
            }

            context.Items["Tenant"] = tenant;

            await _next(context);
        }
    }
}