using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using UpDiddyApi.ApplicationCore.Exceptions;
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = loggerFactory?.CreateLogger<ExceptionMiddleware>() ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AlreadyExistsException ex)
        {
            await CreateResponse(400, ex, context);
        }
        catch (MaximumReachedException ex)
        {
            await CreateResponse(400, ex, context);
        }
        catch (NotFoundException ex)
        {
            await CreateResponse(400, ex, context);
        }
        catch (NotSupportedException ex)
        {
            await CreateResponse(400, ex, context);
        }
        catch (NullReferenceException ex)
        {
            await CreateResponse(400, ex, context);
        }
        catch (ExpiredJobException ex)
        {
            await CreateResponse(410, ex, context);
        }
        catch (Exception ex)
        {
            await CreateResponse(400, ex, context);
            _logger.Log(LogLevel.Error, $"Unhandled exception thrown -> {ex.InnerException}");
            throw;
        }
    }

    private async Task CreateResponse(int statusCode, Exception ex, HttpContext context)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = @"application/json";
        await context.Response.WriteAsync(ex.Message);
        return;
    }
}

// Extension method used to add the middleware to the HTTP request pipeline.
public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionMiddleware>();
    }
}