using System.Text;

namespace Api.Middleware;

public class WebhookLoggingMiddleware(RequestDelegate next, ILogger<WebhookLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Aplicar apenas para rotas de webhook
        if (context.Request.Path.StartsWithSegments("/api/webhooks"))
        {
            await LogWebhookRequest(context);
        }
        else
        {
            await next(context);
        }
    }
    
    private async Task LogWebhookRequest(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;
        
        try
        {
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            var startTime = DateTime.UtcNow;
            
            logger.LogInformation("Webhook recebido: {Method} {Path} - Headers: {Headers}", 
                context.Request.Method, 
                context.Request.Path,
                string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}: {h.Value}")));

            await next(context);

            var duration = DateTime.UtcNow - startTime;
            
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            logger.LogInformation("Webhook processado: {StatusCode} em {Duration}ms - Response: {Response}", 
                context.Response.StatusCode, 
                duration.TotalMilliseconds,
                responseText);

            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro no middleware de logging do webhook");
            context.Response.Body = originalBodyStream;
            throw;
        }
    }

}
