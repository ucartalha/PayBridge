using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using PayBridge.Api.Errors;
using PayBridge.BuildingBlocks.Exceptions;

namespace PayBridge.Api.Exceptions;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IErrorCatalog _errorCatalog;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IErrorCatalog errorCatalog)
    {
        _logger = logger;
        _errorCatalog = errorCatalog;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = GetStatusCode(exception);

        LogException(
            httpContext,
            exception,
            statusCode);

        var response = CreateErrorResponse(
            httpContext,
            exception,
            statusCode);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(
            response,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        await httpContext.Response.WriteAsync(
            json,
            cancellationToken);

        return true;
    }

    private void LogException(
        HttpContext httpContext,
        Exception exception,
        int statusCode)
    {
        switch (exception)
        {
            case BusinessException businessException:
                _logger.LogInformation(
                    "Business request rejected. " +
                    "TraceId: {TraceId}, StatusCode: {StatusCode}, " +
                    "ErrorCode: {ErrorCode}",
                    httpContext.TraceIdentifier,
                    statusCode,
                    businessException.ErrorCode);

                break;

            case PayBridge.BuildingBlocks.Exceptions.ValidationException:
                _logger.LogInformation(
                    "Request validation failed. " +
                    "TraceId: {TraceId}, StatusCode: {StatusCode}",
                    httpContext.TraceIdentifier,
                    statusCode);

                break;

            case UnauthorizedAccessException:
                _logger.LogWarning(
                    "Unauthorized request. " +
                    "TraceId: {TraceId}, StatusCode: {StatusCode}",
                    httpContext.TraceIdentifier,
                    statusCode);

                break;

            default:
                _logger.LogError(
                    exception,
                    "Unhandled exception occurred. " +
                    "TraceId: {TraceId}, StatusCode: {StatusCode}",
                    httpContext.TraceIdentifier,
                    statusCode);

                break;
        }
    }

    private ApiErrorResponse CreateErrorResponse(
        HttpContext httpContext,
        Exception exception,
        int statusCode)
    {
        if (exception is BusinessException businessException)
        {
            var descriptor = _errorCatalog.GetByCode(
                businessException.ErrorCode);

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

        if (exception is
            PayBridge.BuildingBlocks.Exceptions.ValidationException
            validationException)
        {
            return new ApiErrorResponse
            {
                StatusCode = statusCode,
                TraceId = httpContext.TraceIdentifier,
                Error = new ApiError
                {
                    Code = 90001,
                    Key = "validation.failed",
                    Message = "Gönderilen bilgiler geçersiz.",
                    ValidationErrors = validationException.Errors
                }
            };
        }

        if (exception is UnauthorizedAccessException)
        {
            return new ApiErrorResponse
            {
                StatusCode = statusCode,
                TraceId = httpContext.TraceIdentifier,
                Error = new ApiError
                {
                    Code = 90002,
                    Key = "authorization.unauthorized",
                    Message = "Bu işlem için yetkiniz bulunmuyor."
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
                Message = "İşlem sırasında bilinmeyen bir hata oluştu."
            }
        };
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            BusinessException =>
                StatusCodes.Status400BadRequest,

            PayBridge.BuildingBlocks.Exceptions.ValidationException =>
                StatusCodes.Status400BadRequest,

            UnauthorizedAccessException =>
                StatusCodes.Status401Unauthorized,

            _ =>
                StatusCodes.Status500InternalServerError
        };
    }
}