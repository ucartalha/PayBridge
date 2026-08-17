using PayBridge.Modules.Providers.Contracts;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
namespace PayBridge.Modules.Providers.Infrastructure.Decorators
{
    public sealed class LoggingPaymentProviderDecorator : IPaymentProvider
    {
        private readonly IPaymentProvider _inner;
        private readonly ILogger<LoggingPaymentProviderDecorator> _logger;
        public LoggingPaymentProviderDecorator(IPaymentProvider inner, ILogger<LoggingPaymentProviderDecorator> logger)
        {
            _inner = inner;
            _logger = logger;
        }
        public string ProviderCode => _inner.ProviderCode;

        public async Task<ProviderChargeResponse> ChargeAsync(ProviderChargeRequest request, CancellationToken cancellationToken = default)
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                var response =await _inner.ChargeAsync(request, cancellationToken);

                stopWatch.Stop();
                _logger.LogInformation(
                    "Provider request completed. " +
                    "ProviderCode: {ProviderCode}, " +
                    "PaymentId: {PaymentId}, " +
                    "DurationMs: {DurationMs}" +
                    "ErrorCode: {ErrorCode}, " +
                    ProviderCode,
                    request.PaymentId,
                    stopWatch.ElapsedMilliseconds,
                    response.ErrorCode
                    );

                return response;

            }
            catch (OperationCanceledException)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    stopWatch.Stop();
                    _logger.LogInformation(
               "Provider charge cancelled by request. " +
               "ProviderCode: {ProviderCode}, " +
               "PaymentId: {PaymentId}, " +
               "DurationMs: {DurationMs}",
               ProviderCode,
               request.PaymentId,
               stopWatch.ElapsedMilliseconds);
                    throw;
                }
                throw;
            }
            catch(Exception exception) 
            { 
                stopWatch.Stop();
                _logger.LogWarning(
                    "Provider charge failed unexpectedly. " +
                    "ProviderCode: {ProviderCode}, " +
                    "PaymentId: {PaymentId}, " +
                    "DurationMs: {DurationMs}, " +
                    "ExceptionType: {ExceptionType}",
                    ProviderCode,
                    request.PaymentId,
                    stopWatch.ElapsedMilliseconds,
                    exception.GetType().Name);
                throw;
            }

        }

        public async Task<ProviderInquiryResponse> InquiryAsync(ProviderInquiryRequest request, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response =await _inner.InquiryAsync(
                    request,
                    cancellationToken);

                stopwatch.Stop();

                _logger.LogInformation(
                    "Provider inquiry completed. " +
                    "ProviderCode: {ProviderCode}, " +
                    "PaymentId: {PaymentId}, " +
                    "ProviderState: {ProviderState}, " +
                    "DurationMs: {DurationMs}, " +
                    "ErrorCode: {ErrorCode}",
                    ProviderCode,
                    request.PaymentId,
                    response.State,
                    stopwatch.ElapsedMilliseconds,
                    response.ErrorCode);

                return response;
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "Provider inquiry cancelled by request. " +
                    "ProviderCode: {ProviderCode}, " +
                    "PaymentId: {PaymentId}, " +
                    "DurationMs: {DurationMs}",
                    ProviderCode,
                    request.PaymentId,
                    stopwatch.ElapsedMilliseconds);

                throw;
            }
            catch (Exception exception)
            {
                stopwatch.Stop();

                _logger.LogError(
                    exception,
                    "Provider inquiry failed unexpectedly. " +
                    "ProviderCode: {ProviderCode}, " +
                    "PaymentId: {PaymentId}, " +
                    "DurationMs: {DurationMs}",
                    ProviderCode,
                    request.PaymentId,
                    stopwatch.ElapsedMilliseconds);

                throw;
            }
        }
    }
}
