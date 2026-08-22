using System.Security.Claims;
using api_infor_cell.src.Configuration;
using api_infor_cell.src.Shared.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace api_infor_cell.src.Middleware
{
    public class ApiLoggerMiddleware(AppDbContext appDbContext) : IAsyncActionFilter
    {
        private static readonly HashSet<string> IgnoredPaths =
        [
            "/api/loggers",
            "/api/check",
            "/api/auth/login",
            "/api/auth/register",
            "/api/notifications/send"
        ];

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            ActionExecutedContext executed = await next();

            sw.Stop();

            string path = context.HttpContext.Request.Path.Value?.ToLower() ?? "";

            if (IgnoredPaths.Any(p => path.StartsWith(p))) return;

            int statusCode = executed.Result switch
            {
                ObjectResult obj => obj.StatusCode ?? 200,
                StatusCodeResult sc => sc.StatusCode,
                _ => context.HttpContext.Response.StatusCode
            };

            string message = ExtractMessage(executed.Result) ?? ResolveDefaultMessage(statusCode);
            object? responseResult = ExtractResponseResult(executed.Result);

            string userId = context.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            string? plan = context.HttpContext.User?.FindFirst("plan")?.Value ?? "";
            string? company = context.HttpContext.User?.FindFirst("company")?.Value ?? "";
            string? store = context.HttpContext.User?.FindFirst("store")?.Value ?? "";

            string method = context.HttpContext.Request.Method.ToUpper();

            if (executed.Exception is not null && !executed.ExceptionHandled)
            {
                message = executed.Exception.Message;
                statusCode = 500;
            }

            await appDbContext.ApiLogs.InsertOneAsync(new()
            {
                Collection = "",
                Company = company,
                Store = store,
                Plan = plan,
                CreatedBy = userId,
                UpdatedBy = userId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Message = message,
                Path = context.HttpContext.Request.Path,
                Status = statusCode,
                Method = method,
                Time = Math.Round(sw.Elapsed.TotalSeconds, 3),
                Active = true,
                Deleted = false
            });
        }

        private static string? ExtractMessage(IActionResult? result)
        {
            if (result is not ObjectResult obj || obj.Value is null) return null;

            var value = obj.Value;
            var type = value.GetType();

            var messageProp = type.GetProperty("Message") ?? type.GetProperty("message");

            if (messageProp is not null) return messageProp.GetValue(value)?.ToString();

            var resultProp = type.GetProperty("Result") ?? type.GetProperty("result");

            if (resultProp is not null)
            {
                var inner = resultProp.GetValue(value);
                var innerMsg = inner?.GetType().GetProperty("Message") ?? inner?.GetType().GetProperty("message");
                if (innerMsg is not null) return innerMsg.GetValue(inner)?.ToString();
            }

            return null;
        }

        private static string ResolveDefaultMessage(int statusCode) => statusCode switch
        {
            200 => "OK",
            201 => "Criado com sucesso",
            204 => "Excluído com sucesso",
            400 => "Requisição inválida",
            401 => "Não autorizado",
            403 => "Acesso negado",
            404 => "Não encontrado",
            500 => "Erro interno",
            _ => $"Status {statusCode}"
        };

        private static object? ExtractResponseResult(IActionResult? result)
        {
            if (result is not ObjectResult obj || obj.Value is null) return null;
            var value = obj.Value;
            var type = value.GetType();
            var resultProp = type.GetProperty("Result") ?? type.GetProperty("result");
            if (resultProp != null)
            {
                return resultProp.GetValue(value);
            }
            return value;
        }
    }
}