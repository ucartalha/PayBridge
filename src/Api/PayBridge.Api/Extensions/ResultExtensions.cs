using Microsoft.AspNetCore.Mvc;
using PayBridge.Api.Errors;
using PayBridge.Api.Exceptions;
using PayBridge.BuildingBlocks.Results;

namespace PayBridge.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(
        this Result<T> result,
        ControllerBase controller,
        IErrorCatalog errorCatalog)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(result.Value);
        }

        var descriptor =
            errorCatalog.GetByCode(
                result.ErrorCode!.Value);

        var statusCode = result.Status switch
        {
            ResultStatus.Conflict =>
                StatusCodes.Status409Conflict,

            ResultStatus.BusinessFailure =>
                StatusCodes.Status400BadRequest,

            _ =>
                StatusCodes.Status400BadRequest
        };

        return controller.StatusCode(
            statusCode,
            new ApiErrorResponse
            {
                StatusCode = statusCode,
                TraceId = controller.HttpContext.TraceIdentifier,
                Error = new ApiError
                {
                    Code = descriptor.Code,
                    Key = descriptor.Key,
                    Message = descriptor.UserMessage
                }
            });
    }
}