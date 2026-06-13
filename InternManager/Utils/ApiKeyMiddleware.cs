namespace InternManager.Utils
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _check;
        private const string API_KEY_HEADER_NAME = "X-API-Key";

        public ApiKeyMiddleware(RequestDelegate check)
        {
            _check = check;
        }

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            if (!context.Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var extractedApiKey))
            {
                context.Response.StatusCode = 401; 
                context.Response.ContentType = "text/plain; charset=utf-8";
                await context.Response.WriteAsync("Thất bại: Bạn phải cung cấp API Key để kết nối hệ thống.");
                return;
            }

            var sytemApiKey = configuration["Settings:SecretKey"];

            if (!sytemApiKey.Equals(extractedApiKey))
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "text/plain; charset=utf-8";
                await context.Response.WriteAsync("Thất bại: API Key không chính xác. Từ chối kết nối.");
                return;
            }

            await _check(context);
        }
    }
}
