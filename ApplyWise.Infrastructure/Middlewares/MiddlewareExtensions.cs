using ApplyWise.Domain.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace ApplyWise.Infrastructure.Middlewares;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(static appBuilder =>
        {
            appBuilder.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

                int statusCode;
                string message;

                switch (exception)
                {
                    case ExternalServiceException ex:
                        statusCode = 502;
                        message = ex.Message;
                        break;
                    case NotFoundException ex:
                        statusCode = 404;
                        message = ex.Message;
                        break;
                    case ArgumentException ex:
                        statusCode = 400;
                        message = ex.Message;
                        break;
                    default:
                        statusCode = 500;
                        message = "Erro interno inesperado.";
                        break;
                }

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = message });
            });
        });

        return app;
    }
}
