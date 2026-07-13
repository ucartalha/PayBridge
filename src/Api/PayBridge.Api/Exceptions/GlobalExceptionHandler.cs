using Microsoft.AspNetCore.Diagnostics;
using PayBridge.Api.Errors;
using PayBridge.BuildingBlocks.Exceptions;
using System.Text.Json;

namespace PayBridge.Api.Exceptions
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IErrorCatalog _errorCatalog;
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IErrorCatalog errorCatalog)
        {
            _errorCatalog = errorCatalog;
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var statusCode = GetStatusCode(exception);
            
            var response = CreateErrorResponse(httpContext, exception, statusCode);

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            _logger.LogError(exception,
                "Unhandled Exception occured. TraceId: {TraceId}, StatusCode: {StatusCode}",
                httpContext.TraceIdentifier,
                statusCode
                );
            var json = JsonSerializer.Serialize(response,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            await httpContext.Response.WriteAsync(json, cancellationToken);

            return true;

        }

        private ApiErrorResponse CreateErrorResponse(HttpContext httpContext,
            Exception exception,
            int statusCode)
        {
            if (exception is BusinessException businessException)
            {
                var descriptor = _errorCatalog.GetByCode(businessException.ErrorCode);
                return new ApiErrorResponse
                {
                    StatusCode = statusCode,
                    TraceId = httpContext.TraceIdentifier,
                    Error = new ApiError
                    {
                        Code = descriptor.Code,
                        Key = descriptor.Key,
                        Message = descriptor.UserMessage
                    }
                };
            }
            if (exception is PayBridge.BuildingBlocks.Exceptions.ValidationException validationException)
            {
                return new ApiErrorResponse
                {
                    StatusCode = statusCode,
                    TraceId = httpContext.TraceIdentifier,
                    Error = new ApiError
                    {
                        Code = 90001,
                        Key = "validation.failed",
                        Message = "Gönderilen bilgiler geçersiz",
                        ValidationErrors = validationException.Errors
                    }
                };

            }


            return new ApiErrorResponse
                {
                    StatusCode = statusCode,
                    TraceId = httpContext.TraceIdentifier,
                    Error = new ApiError
                    {
                        Code = 9000,
                        Key = "system.unhandled.exception",
                        Message = "işlem sırasında bilinmeyen bir hata oluştu"
                    }
                };
            }
        
        private static int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                BusinessException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                PayBridge.BuildingBlocks.Exceptions.ValidationException=> StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError

            };
        }
    }

   
}
