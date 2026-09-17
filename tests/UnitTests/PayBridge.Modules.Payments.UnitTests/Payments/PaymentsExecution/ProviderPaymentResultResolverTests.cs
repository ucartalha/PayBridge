using PayBridge.Modules.Payments.Application.Payments.PaymentsExecution;
using PayBridge.Modules.Providers.Contracts;
using PayBridge.Modules.Providers.Contracts.Enums;
using PayBridge.Modules.Providers.Infrastructure.Mock;

namespace PayBridge.Modules.Payments.UnitTests.Payments.PaymentsExecution;

public sealed class ProviderPaymentResultResolverTests
{
    [Fact]
    public async Task ResolveAsync_WhenChargeStillProcessing_PropagatesCredentialToInquiry()
    {
        var credential = CreateCredential();
        var provider = new RecordingPaymentProvider
        {
            ChargeHandler = _ => ProviderChargeResponse.InquiryRequired(
                "PROVIDER_TIMEOUT",
                "Provider timeout."),
            InquiryHandler = _ => ProviderInquiryResponse.Success("provider-tx-1")
        };

        var resolver = new ProviderPaymentResultResolver();

        var result = await resolver.ResolveAsync(provider, CreateChargeRequest(credential));

        Assert.Equal(ProviderPaymentState.Succeeded, result.State);
        Assert.Equal("provider-tx-1", result.ProviderTransactionId);
        Assert.Same(credential, provider.InquiryRequests.Single().Credential);
    }

    [Fact]
    public async Task ResolveAsync_WhenMultipleInquiryAttemptsExecute_PropagatesSameCredentialToEveryInquiry()
    {
        var credential = CreateCredential();
        var provider = new RecordingPaymentProvider
        {
            ChargeHandler = _ => ProviderChargeResponse.InquiryRequired(
                "PROCESSING",
                "Provider is still processing."),
            InquiryHandler = request => request.AttemptNumber < 3
                ? ProviderInquiryResponse.StillProcessing(
                    "PROCESSING",
                    "Provider is still processing.")
                : ProviderInquiryResponse.Success("provider-tx-3")
        };

        var resolver = new ProviderPaymentResultResolver();

        var result = await resolver.ResolveAsync(provider, CreateChargeRequest(credential));

        Assert.Equal(ProviderPaymentState.Succeeded, result.State);
        Assert.Equal("provider-tx-3", result.ProviderTransactionId);
        Assert.Equal(3, provider.InquiryRequests.Count);
        Assert.All(provider.InquiryRequests, request =>
        {
            Assert.Same(credential, request.Credential);
            Assert.Equal(credential.MerchantProviderAccountId, request.Credential!.MerchantProviderAccountId);
            Assert.Equal(credential.ProviderCode, request.Credential.ProviderCode);
            Assert.Equal(credential.CredentialPayloadJson, request.Credential.CredentialPayloadJson);
        });
    }

    [Fact]
    public async Task ResolveAsync_WhenChargeTimesOut_PreservesCredentialAndCallsChargeOnce()
    {
        var credential = CreateCredential();
        var provider = new RecordingPaymentProvider
        {
            ChargeHandler = _ => throw new TimeoutException("Charge timed out."),
            InquiryHandler = _ => ProviderInquiryResponse.Success("provider-tx-2")
        };

        var resolver = new ProviderPaymentResultResolver();

        var result = await resolver.ResolveAsync(provider, CreateChargeRequest(credential));

        Assert.Equal(ProviderPaymentState.Succeeded, result.State);
        Assert.Equal("provider-tx-2", result.ProviderTransactionId);
        Assert.Equal(1, provider.ChargeCallCount);
        Assert.Same(credential, provider.InquiryRequests.Single().Credential);
    }

    [Fact]
    public async Task ResolveAsync_WhenChargeCredentialIsNull_InquiryCredentialRemainsNull()
    {
        var provider = new RecordingPaymentProvider
        {
            ChargeHandler = _ => ProviderChargeResponse.InquiryRequired(
                "PROCESSING",
                "Provider is still processing."),
            InquiryHandler = _ => ProviderInquiryResponse.Success("provider-tx-null")
        };

        var resolver = new ProviderPaymentResultResolver();

        var result = await resolver.ResolveAsync(provider, CreateChargeRequest());

        Assert.Equal(ProviderPaymentState.Succeeded, result.State);
        Assert.Null(provider.InquiryRequests.Single().Credential);
    }

    [Fact]
    public async Task ResolveAsync_WhenCallerCancellationOccurs_DoesNotConvertToInquiryRecovery()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        var provider = new RecordingPaymentProvider
        {
            ChargeHandler = _ => throw new TaskCanceledException("Caller cancelled.")
        };

        var resolver = new ProviderPaymentResultResolver();

        await Assert.ThrowsAsync<TaskCanceledException>(
            () => resolver.ResolveAsync(
                provider,
                CreateChargeRequest(CreateCredential()),
                cancellationTokenSource.Token));

        Assert.Equal(1, provider.ChargeCallCount);
        Assert.Empty(provider.InquiryRequests);
    }

    [Theory]
    [InlineData(99, ProviderPaymentState.Succeeded, 2)]
    [InlineData(98, ProviderPaymentState.Failed, 3)]
    [InlineData(97, ProviderPaymentState.StillProcessing, 3)]
    public async Task ResolveAsync_WithMockRecoveryScenarios_PreservesRecoveryBehaviorAndCallsChargeOnce(
        decimal amount,
        ProviderPaymentState expectedState,
        int expectedInquiryCount)
    {
        var provider = new RecordingPaymentProvider(new MockPaymentProvider());
        var resolver = new ProviderPaymentResultResolver();

        var result = await resolver.ResolveAsync(
            provider,
            CreateChargeRequest(
                credential: CreateCredential(),
                amount: amount));

        Assert.Equal(expectedState, result.State);
        Assert.Equal(1, provider.ChargeCallCount);
        Assert.Equal(expectedInquiryCount, provider.InquiryRequests.Count);
    }

    private static ProviderCredentialContext CreateCredential()
    {
        return new ProviderCredentialContext(
            MerchantProviderAccountId: Guid.NewGuid(),
            ProviderCode: "Mock",
            CredentialPayloadJson: """{"apiKey":"secret","terminalId":"terminal-1"}""");
    }

    private static ProviderChargeRequest CreateChargeRequest(
        ProviderCredentialContext? credential = null,
        decimal amount = 99m)
    {
        var paymentId = Guid.NewGuid();

        return new ProviderChargeRequest(
            PaymentId: paymentId,
            OrderId: "order-1",
            Amount: amount,
            Currency: "TRY",
            IdempotencyKey: paymentId.ToString("N"),
            Credential: credential);
    }

    private sealed class RecordingPaymentProvider : IPaymentProvider
    {
        private readonly IPaymentProvider? _inner;

        public RecordingPaymentProvider()
        {
        }

        public RecordingPaymentProvider(IPaymentProvider inner)
        {
            _inner = inner;
        }

        public string ProviderCode => "Mock";

        public int ChargeCallCount { get; private set; }

        public List<ProviderInquiryRequest> InquiryRequests { get; } = [];

        public Func<ProviderChargeRequest, ProviderChargeResponse> ChargeHandler { get; init; } =
            _ => ProviderChargeResponse.Success("provider-tx");

        public Func<ProviderInquiryRequest, ProviderInquiryResponse> InquiryHandler { get; init; } =
            _ => ProviderInquiryResponse.StillProcessing("PROCESSING", "Processing.");

        public async Task<ProviderChargeResponse> ChargeAsync(
            ProviderChargeRequest request,
            CancellationToken cancellationToken = default)
        {
            ChargeCallCount++;

            if (_inner is not null)
            {
                return await _inner.ChargeAsync(request, cancellationToken);
            }

            return ChargeHandler(request);
        }

        public Task<ProviderInquiryResponse> InquiryAsync(
            ProviderInquiryRequest request,
            CancellationToken cancellationToken = default)
        {
            InquiryRequests.Add(request);

            if (_inner is not null)
            {
                return _inner.InquiryAsync(request, cancellationToken);
            }

            return Task.FromResult(InquiryHandler(request));
        }
    }
}
