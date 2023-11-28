using InvestmentPortfolioManager.Exceptions;
using Newtonsoft.Json;

namespace InvestmentPortfolioManager.Middleware
{
    public class ErrorHandlingMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (NotFoundException notFoundException)
            {
                context.Response.StatusCode = 404;
                await HandleExceptionAsync(context, notFoundException);
            }
            catch (BadRequestException badRequestException)
            {
                context.Response.StatusCode = 400;
                await HandleExceptionAsync(context, badRequestException);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var result = JsonConvert.SerializeObject(new { error = exception.Message });
            context.Response.ContentType = "application/json";

            return context.Response.WriteAsync(result); 
        }
    }
}
