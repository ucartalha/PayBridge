using MediatR;
using Microsoft.Extensions.Logging;
using PayBridge.BuildingBlocks.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.BuildingBlocks.CQRS.Behaviors
{
    public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
        public LoggingBehavior(ILogger<LoggingBehavior<TRequest,TResponse>> logger)
        {
            _logger = logger;   
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;    
            var stopWatch = Stopwatch.StartNew();
            _logger.LogInformation("CQRS Request started : {RequestName}", requestName);
            try
            {
                var response = await next();
                stopWatch.Stop();
                _logger.LogInformation(
                "CQRS request completed. RequestName: {RequestName}, DurationMs: {DurationMs}",
                requestName,
                stopWatch.ElapsedMilliseconds);
                return response;
            }
            catch (Exception exception)
            {
                stopWatch.Stop();

                if (exception is BusinessException or ValidationException)
                {
                    _logger.LogInformation(
                        "CQRS request rejected. " +
                        "RequestName: {RequestName}, DurationMs: {DurationMs}, " +
                        "ExceptionType: {ExceptionType}",
                        requestName,
                        stopWatch.ElapsedMilliseconds,
                        exception.GetType().Name);
                }
                else
                {
                    _logger.LogWarning(
                        "CQRS request failed unexpectedly. " +
                        "RequestName: {RequestName}, DurationMs: {DurationMs}, " +
                        "ExceptionType: {ExceptionType}",
                        requestName,
                        stopWatch.ElapsedMilliseconds,
                        exception.GetType().Name);
                }

                throw;
            }

        }
    }
}
