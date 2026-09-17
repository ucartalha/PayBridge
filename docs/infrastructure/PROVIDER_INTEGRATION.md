# PayBridge Provider Integration

**Document status:** IMPLEMENTED FOUNDATION + INTEGRATION STANDARD + KNOWN RISKS + TARGET  
**Target path:** `docs/infrastructure/PROVIDER_INTEGRATION.md`  
**Repository:** `ucartalha/PayBridge`  
**Reviewed branch:** `main`  
**Reviewed commit:** `bb696739e98a46de0af6829b24c9948491d9d329`  
**Reviewed commit message:** `refund started and logging improvements`  
**Review date:** `2026-09-17`

> This document defines how payment service providers (PSPs) must be integrated into PayBridge.
>
> It describes the current provider abstraction, dependency boundaries, merchant-provider account and credential model, adapter responsibilities, registration/decorator structure, Charge/Inquiry behavior, error normalization, logging requirements, testing requirements, and the production-readiness checklist for adding a real provider.
>
> This is not a generic provider-integration guide. It is specific to the current PayBridge repository and architecture.

---

# 1. Purpose

PayBridge is intended to support multiple payment providers without allowing provider-specific implementation details to leak into the Payments domain or orchestration layer.

The required direction is:

```text
Payments
    ↓
Providers.Contracts
    ↓
IPaymentProvider
    ↓
Providers.Infrastructure
    ↓
Provider Adapter
    ↓
External PSP
```

The Payments module should understand:

```text
Charge
Inquiry
Provider state
Provider transaction id
Provider error
Provider credential context
```

It should NOT understand:

```text
provider-specific HTTP URLs
provider-specific headers
provider-specific access-token response DTOs
provider-specific signature algorithms
provider SDK classes
provider-native request/response formats
```

Those concerns belong inside provider infrastructure/adapters.

---

# 2. Current Capability Status

| Capability | Status |
|---|---|
| Provider Contracts project | IMPLEMENTED |
| Provider Infrastructure project | IMPLEMENTED |
| `IPaymentProvider` abstraction | IMPLEMENTED |
| `IPaymentProviderFactory` abstraction | IMPLEMENTED |
| Provider selection by `ProviderCode` | IMPLEMENTED |
| Case-insensitive provider factory lookup | IMPLEMENTED |
| Logging decorator | IMPLEMENTED |
| Mock provider | IMPLEMENTED |
| Charge contract | IMPLEMENTED |
| Inquiry contract | IMPLEMENTED |
| Inquiry receives resolved credential context | IMPLEMENTED |
| Merchant provider account model | IMPLEMENTED |
| Merchant provider credential model | IMPLEMENTED |
| Credential encryption abstraction | IMPLEMENTED |
| Development DataProtection credential protector | IMPLEMENTED |
| Active provider-account reader | IMPLEMENTED |
| Active credential reader | IMPLEMENTED |
| Channel capability filtering | IMPLEMENTED |
| Refund capability flag | IMPLEMENTED |
| Provider priority field | IMPLEMENTED MODEL / NOT ROUTING |
| Real PSP adapter | TARGET |
| HttpClientFactory provider clients | TARGET |
| Provider-specific DTO/mapping layer | TARGET |
| Provider access-token lifecycle | TARGET |
| Redis provider token cache | TARGET |
| Provider token refresh lock | TARGET |
| Production secret/key management | TARGET |
| Smart routing | TARGET |
| Provider health scoring | TARGET |
| Automatic provider fallback | TARGET |
| Persistent reconciliation | TARGET |
| Full provider refund contract | TARGET |
| Full provider void contract | TARGET |
| Provider webhook contract | TARGET |

---

# 3. Current Repository Structure

Provider-facing code currently lives in:

```text
src/Modules/Providers/
├── PayBridge.Modules.Providers.Contracts/
│   ├── IPaymentProvider.cs
│   ├── IPaymentProviderFactory.cs
│   ├── ProviderChargeRequest.cs
│   ├── ProviderChargeResponse.cs
│   ├── ProviderInquiryRequest.cs
│   ├── ProviderInquiryResponse.cs
│   ├── ProviderCredentialContext.cs
│   ├── Enums/
│   │   └── ProviderPaymentState.cs
│   └── Errors/
│       └── ProviderErrorCode.cs
│
├── PayBridge.Modules.Providers.Application/
│
└── PayBridge.Modules.Providers.Infrastructure/
    ├── DependencyInjection.cs
    ├── PaymentProviderFactory.cs
    ├── Decorators/
    │   └── LoggingPaymentProviderDecorator.cs
    └── Mock/
        └── MockPaymentProvider.cs
```

Provider credential ownership currently lives in the Merchant module:

```text
src/Modules/Merchants/
├── PayBridge.Modules.Merchants.Domain/
│   └── Merchants/Entities/
│       ├── MerchantProviderAccount.cs
│       └── MerchantProviderCredential.cs
│
├── PayBridge.Modules.Merchants.Contracts/
│   ├── Credentials/
│   │   └── ICredentialProtector.cs
│   └── Merchants/
│       ├── MerchantProviderAccountInfo.cs
│       └── MerchantProviderCredentialInfo.cs
│
└── PayBridge.Modules.Merchants.Infrastructure/
    ├── Persistence/Readers/
    │   └── MerchantProviderAccountReader.cs
    └── Security/
        └── DevelopmentCredentialProtector.cs
```

Payments resolves credentials through:

```text
src/Modules/Payments/
└── PayBridge.Modules.Payments.Infrastructure/
    └── PaymentsExecution/
        └── ProviderCredentialResolver.cs
```

---

# 4. Architectural Boundary

The current provider boundary is intentionally contract-based.

`Providers.Infrastructure` references:

```text
Providers.Contracts
```

and does not require:

```text
Payments.Domain
Merchants.Domain
```

for its adapter implementation.

This is the preferred direction.

Provider adapters must remain replaceable infrastructure components.

---

# 5. Current Dependency Direction

Preferred dependency graph:

```text
Payments.Application
        ↓
Providers.Contracts

Providers.Infrastructure
        ↓
Providers.Contracts
```

Merchant/provider credential lookup is exposed to Payments through Merchant contracts/readers.

A provider adapter should receive already-resolved provider-neutral credentials.

---

# 6. Forbidden Dependency Direction

Do not make provider infrastructure depend on:

```text
Payments.Infrastructure
PaymentsDbContext
Payment aggregate internals
Merchants.Infrastructure
MerchantDbContext
```

A provider adapter should not query PayBridge databases directly.

Its responsibility is external provider communication.

---

# 7. Current Payments Dependency Warning

The current `Payments.Application` project references:

```text
Merchants.Contracts
Providers.Contracts
```

which is expected.

However, due to the incomplete Refund work, it also currently references:

```text
Merchants.Domain
```

This is not the pattern new provider integrations should copy.

Provider integration code should use contracts/abstractions rather than introducing additional direct domain coupling across modules.

---

# 8. IPaymentProvider

Current core provider abstraction:

```csharp
public interface IPaymentProvider
{
    string ProviderCode { get; }

    Task<ProviderChargeResponse> ChargeAsync(
        ProviderChargeRequest request,
        CancellationToken cancellationToken = default);

    Task<ProviderInquiryResponse> InquiryAsync(
        ProviderInquiryRequest request,
        CancellationToken cancellationToken = default);
}
```

A real provider adapter must implement this contract.

---

# 9. Current Supported Operations

Current provider contract supports:

```text
Charge
Inquiry
```

It does NOT yet support:

```text
RefundAsync
VoidAsync
Webhook verification
Provider token issuance
```

Those should be added only when their workflows and transaction boundaries are designed.

Do not extend the interface casually with provider-specific operations.

---

# 10. ProviderCode

Every provider exposes:

```text
ProviderCode
```

Current Mock provider:

```text
Mock
```

The provider factory uses this code to resolve an adapter.

The same code also appears in:

```text
Payment
PaymentTransaction
MerchantProviderAccount
PaymentExecutionRequest
```

ProviderCode is therefore an important cross-module identifier.

---

# 11. ProviderCode Canonicalization

Current factory comparison:

```text
StringComparison.OrdinalIgnoreCase
```

Therefore:

```text
mock
Mock
MOCK
```

resolve to the same registered adapter.

Merchant provider-account lookup currently:

```text
Trim()
```

normalizes the incoming value and compares it to persisted `ProviderCode`.

Database case sensitivity then depends on SQL collation.

This creates a potential normalization difference.

---

# 12. Recommended ProviderCode Rule

A future provider onboarding standard should define a canonical code format.

Recommended conceptual rule:

```text
ProviderCode:
stable
short
ASCII
case-normalized
never user-facing display text
```

For example:

```text
PAYTR
IYZICO
PARAM
SIPAY
```

or another consistent convention.

Once persisted in payments, provider codes should not be renamed casually.

---

# 13. ProviderCode Is Durable Identity

Current `Payment` persists:

```text
ProviderCode
```

Current `PaymentTransaction` also persists:

```text
ProviderCode
```

This is important for:

- Inquiry
- reconciliation
- support
- refund
- void
- reporting
- provider performance metrics

Future smart routing must persist the selected provider BEFORE sending Charge.

---

# 14. IPaymentProviderFactory

Current factory abstraction:

```text
IPaymentProviderFactory
```

Current implementation receives:

```text
IEnumerable<IPaymentProvider>
```

and finds one whose:

```text
ProviderCode
```

matches the requested code.

If no provider exists:

```text
ProviderErrorCode.ProviderNotSupported
```

is raised as a BusinessException.

---

# 15. Provider Factory Is Not a Routing Engine

Today:

```text
request.ProviderCode
    ↓
factory.Resolve(providerCode)
```

The factory performs adapter lookup.

It does NOT calculate:

```text
best provider
cheapest provider
highest success rate provider
healthy provider
merchant priority provider
fallback provider
```

Do not place future smart-routing policy directly inside `PaymentProviderFactory`.

---

# 16. Future Routing Layer

Target conceptual model:

```text
Merchant
    ↓
Eligible MerchantProviderAccounts
    ↓
Routing Policy
    ↓
Selected ProviderCode
    ↓
IPaymentProviderFactory.Resolve(...)
    ↓
Adapter
```

Routing decides WHICH provider.

Factory returns the IMPLEMENTATION for that provider.

These are separate responsibilities.

---

# 17. MerchantProviderAccount

Current merchant-provider relationship is modeled by:

```text
MerchantProviderAccount
```

Fields include:

```text
Id
MerchantId
ProviderCode
IsActive
AllowECommerce
AllowPhysicalPos
AllowRefund
Priority
CreatedAtUtc
ActivatedAtUtc
DeactivatedAtUtc
```

---

# 18. Merchant/Provider Uniqueness

Current EF configuration defines:

```text
UNIQUE(MerchantId, ProviderCode)
```

Therefore one merchant currently has at most one provider-account row for a given ProviderCode.

Channels are enabled through flags on that account.

---

# 19. Channel Capability Model

Current provider account supports:

```text
AllowECommerce
AllowPhysicalPos
```

Wallet is currently not allowed by the domain's `EnsureCanProcess`.

This means provider eligibility is merchant-specific and channel-specific.

A provider being globally registered does NOT mean a merchant is allowed to use it.

---

# 20. Refund Capability Model

Current provider account also has:

```text
AllowRefund
```

The domain exposes:

```text
EnsureCanRefund()
ConfigureRefund(...)
```

This is useful for future refund orchestration.

However the current provider contract does not yet expose RefundAsync.

Therefore:

```text
merchant refund capability model: IMPLEMENTED
end-to-end provider refund: PARTIAL/TARGET
```

---

# 21. Provider Priority

`MerchantProviderAccount` has:

```text
Priority
```

and validates:

```text
Priority >= 0
```

Current reader orders eligible account query by:

```text
Priority
```

However current lookup also filters by a specific ProviderCode and database uniqueness allows only one account per merchant/provider.

Therefore Priority currently has little/no routing effect in the active Sale path.

It is best understood as model preparation for future provider routing.

---

# 22. Future Priority Semantics

Before smart routing uses Priority, define:

```text
lower number = higher priority?
or
higher number = higher priority?
```

Current reader uses:

```text
OrderBy(Priority)
```

which implies lower number first.

Document and test this before it becomes routing-critical.

---

# 23. MerchantProviderCredential

Provider credentials are modeled separately from provider accounts.

Current fields:

```text
Id
MerchantProviderAccountId
EncryptedCredentialPayload
EncryptedKeyVersion
IsActive
CreatedAtUtc
RotatedAtUtc
RevokedAtUtc
```

The credential contains a protected JSON payload rather than provider-specific columns.

---

# 24. Why Credential Payload Is JSON

Using:

```text
CredentialPayloadJson
```

allows different providers to require different secret fields without adding all provider-specific credential columns to the Merchant domain.

Example conceptual payloads:

```json
{
  "merchantId": "...",
  "apiKey": "...",
  "secretKey": "..."
}
```

or:

```json
{
  "clientId": "...",
  "clientSecret": "...",
  "terminalId": "..."
}
```

The exact shape belongs to the provider adapter's interpretation.

---

# 25. Credential Payload Must Stay Provider-Specific

Payments should treat the payload as opaque.

Do not write:

```text
PayTRApiKey
IyzicoSecretKey
ParamClientCode
```

into core Payment entities.

Provider adapter code should deserialize its own expected credential payload.

---

# 26. Credential Uniqueness

Current EF configuration creates a filtered unique index:

```text
MerchantProviderAccountId
WHERE IsActive = 1
```

Therefore each merchant-provider account can have at most one active credential at a time.

This is a strong and useful invariant.

---

# 27. Credential Rotation

Current domain supports:

```text
Rotate(...)
```

which replaces:

```text
EncryptedCredentialPayload
EncryptedKeyVersion
```

and sets:

```text
RotatedAtUtc
```

It also supports:

```text
Revoke()
```

which sets:

```text
IsActive = false
RevokedAtUtc
```

---

# 28. Credential Protection Abstraction

Current abstraction:

```text
ICredentialProtector
```

supports:

```text
Protect(...)
Unprotect(...)
```

using:

```text
credential payload
encryption key version
```

Payments uses this abstraction rather than directly knowing encryption implementation.

---

# 29. Current Credential Protector

Current implementation:

```text
DevelopmentCredentialProtector
```

uses:

```text
ASP.NET Core DataProtection
```

with purpose:

```text
PayBridge.MerchantProviderCredentials.{encryptionKeyVersion}
```

This isolates protected payloads by key-version purpose.

---

# 30. Production Credential Warning

The current class is explicitly named:

```text
DevelopmentCredentialProtector
```

It should not automatically be treated as final production secret-management architecture.

Production requirements must define:

- DataProtection key persistence
- key protection at rest
- key rotation
- multi-instance sharing
- deployment persistence
- backup/recovery
- incident response
- secret auditing

---

# 31. Multi-Instance Credential Requirement

If PayBridge runs on multiple application instances, every instance that may resolve a provider credential must be able to decrypt credentials written by another instance.

Therefore credential protection key material must be shared/persisted appropriately.

An ephemeral per-container key ring would break provider calls after restart/scale-out.

---

# 32. ProviderCredentialResolver

Current Payment Infrastructure resolves provider credentials.

Inputs:

```text
MerchantId
ProviderCode
Channel
```

Flow:

```text
MerchantProviderAccountReader
    ↓
active provider account for merchant/provider/channel
    ↓
active provider credential
    ↓
ICredentialProtector.Unprotect(...)
    ↓
ProviderCredentialContext
```

---

# 33. ProviderCredentialContext

Current provider-neutral runtime credential context:

```text
MerchantProviderAccountId
ProviderCode
CredentialPayloadJson
```

This is passed into:

```text
ProviderChargeRequest
```

A provider adapter can then deserialize `CredentialPayloadJson`.

---

# 34. Sensitive Runtime Object

`ProviderCredentialContext` is highly sensitive.

Never:

```text
log it
serialize it to API responses
store it in Payment error messages
send it to APM tags
include it in exception messages
```

It should exist only long enough for provider communication.

---

# 35. MerchantProviderAccountReader

Current reader resolves an active account by:

```text
MerchantId
ProviderCode
Channel
IsActive
```

It also applies channel flags.

For ECommerce:

```text
AllowECommerce = true
```

For PhysicalPos:

```text
AllowPhysicalPos = true
```

Wallet currently returns no account.

---

# 36. Provider Account Reader Uses AsNoTracking

Current provider-account/credential reads are:

```text
AsNoTracking
```

which is appropriate because payment execution consumes configuration but does not modify the account/credential.

Preserve this read-oriented behavior.

---

# 37. Provider Credential Reader

Current active credential lookup filters:

```text
MerchantProviderAccountId
IsActive = true
```

and projects:

```text
MerchantProviderCredentialInfo
```

rather than returning the Merchant domain entity directly.

This is the preferred cross-module pattern.

---

# 38. Credential DTO Is Internal-Use Sensitive

The contract file explicitly notes that:

```text
MerchantProviderCredentialInfo
```

must not be exposed through API.

Preserve that boundary.

This object contains encrypted credential data and key-version metadata.

---

# 39. ProviderCredentialResolver Error Conditions

Current resolver rejects:

```text
empty MerchantId
empty ProviderCode
provider account not found
credential missing/inactive
```

before the provider call.

These are pre-provider business/configuration failures.

They are not uncertain remote payment outcomes.

---

# 40. Safe Pre-Send Failure

If provider account or credential resolution fails before Charge is sent:

```text
provider definitely did not receive Charge
```

This is materially different from:

```text
Charge timeout
```

Future fallback logic may potentially route another provider after a proven pre-send failure.

That policy does not exist yet.

---

# 41. ProviderChargeRequest Contract

Current canonical Charge request:

```text
PaymentId
OrderId
Amount
Currency
IdempotencyKey
Credential
```

A provider adapter must map these fields to the PSP-specific request.

---

# 42. Provider Charge Idempotency

Current `IdempotencyKey` sent to the provider abstraction is:

```text
PaymentId.ToString("N")
```

This is stable for the PayBridge Payment.

If the PSP supports a merchant-generated idempotency key, the adapter should map this safely.

Do not generate a new provider idempotency key for every HTTP retry.

---

# 43. Provider Idempotency Is Defense-in-Depth

Even when a PSP promises idempotency:

```text
unknown Charge outcome
```

must still follow PayBridge's:

```text
Inquiry/reconciliation
```

model.

Do not use provider idempotency as an excuse for blind Charge retry.

---

# 44. ProviderChargeResponse Contract

Current normalized response:

```text
ProviderPaymentState State
string? ProviderTransactionId
string? ErrorCode
string? ErrorMessage
```

Provider adapters are responsible for converting provider-native responses into this canonical model.

---

# 45. Adapter Normalization Responsibility

Example conceptual PSP response:

```json
{
  "status": "APPROVED",
  "transaction_no": "12345",
  "result_code": "00"
}
```

Adapter should translate to:

```text
ProviderChargeResponse.Success("12345")
```

Payments should never inspect:

```text
APPROVED
00
provider-specific field names
```

---

# 46. Explicit Provider Failure Mapping

An adapter should return:

```text
ProviderChargeResponse.Failed(...)
```

ONLY when the PSP gives a trustworthy terminal failure.

Examples may include:

```text
declined
rejected
invalid card
known terminal provider error
```

depending on provider contract.

---

# 47. Uncertain Provider Mapping

If the adapter cannot safely determine whether the provider processed Charge:

```text
DO NOT map to Failed
```

Use:

```text
StillProcessing / Inquiry-required
```

or throw a transport exception that the recovery layer explicitly classifies as uncertain.

---

# 48. ProviderPaymentState

Current enum:

```text
Unknown
Succeeded
Failed
StillProcessing
Cancelled
Rejected
```

The current Sale resolver only fully canonicalizes:

```text
Succeeded
Failed
StillProcessing
```

before final persistence.

`Cancelled` and `Rejected` semantics must be defined before real adapters return them.

---

# 49. Recommended Adapter Output Rule

Until central semantics are expanded:

```text
terminal provider decline/rejection
→ map to Failed

uncertain/pending
→ map to StillProcessing

success with stable provider transaction id
→ map to Succeeded
```

Only do this when it is semantically correct for that provider.

---

# 50. Success Must Have ProviderTransactionId

PayBridge does not accept:

```text
Succeeded + no transaction id
```

as final success.

Adapter implementers should treat a missing provider reference as abnormal/incomplete.

The resolver will convert it to:

```text
StillProcessing
```

---

# 51. ProviderInquiryRequest Contract

Current Inquiry request:

```text
PaymentId
OrderId
Amount
Currency
ProviderTransactionId
AttemptNumber
Credential
```

Inquiry must be a status/read operation.

It must never create a second financial operation.

---

# 52. Inquiry Credential Propagation

`ProviderInquiryRequest` supports:

```text
Credential
```

Current `ProviderPaymentResultResolver` propagates:

```text
chargeRequest.Credential
```

into short Inquiry requests.

Therefore a resolved:

```text
ProviderCredentialContext
```

is passed to both Charge and short Inquiry.

Provider adapters may use this credential context to authenticate Inquiry calls, but must not log credential payloads, provider access tokens, or secrets.

---

# 53. Inquiry Without ProviderTransactionId

After a network timeout, PayBridge may not possess a PSP transaction id.

A provider integration must define how Inquiry works in this scenario.

Possible provider-supported references:

```text
merchant order id
merchant transaction id
PayBridge PaymentId
provider idempotency key
request reference
```

Before integrating a PSP, confirm that uncertain Charge can be queried safely.

---

# 54. Provider Compatibility Requirement

A PSP that supports Charge but offers no safe way to determine uncertain outcome is operationally dangerous.

Provider onboarding is incomplete until PayBridge can answer:

```text
If Charge response is lost,
how do we discover what happened?
```

---

# 55. Logging Decorator

Every current provider is wrapped by:

```text
LoggingPaymentProviderDecorator
```

The decorator measures:

```text
duration
```

and logs operational metadata.

Current Charge logs:

```text
ProviderCode
PaymentId
ProviderState
DurationMs
ErrorCode
```

Current Inquiry logs equivalent data.

---

# 56. Logging Decorator Must Stay Provider-Neutral

Do not put provider-specific response bodies into the generic logging decorator.

If a provider needs additional diagnostic metadata:

- sanitize it
- classify it
- expose safe normalized fields

Do not dump raw PSP request/response objects into logs.

---

# 57. Sensitive Logging Rule

Never log:

```text
CredentialPayloadJson
API keys
secret keys
client secrets
access tokens
authorization headers
card number
CVV
raw card payload
provider cryptographic signatures containing secrets
```

Provider integration logging must be designed before production.

---

# 58. Current Logging Timeout Nuance

Current logging decorator recognizes caller cancellation:

```text
OperationCanceledException
when request cancellation token is cancelled
```

as cancellation.

But a provider timeout represented as `TaskCanceledException` with caller token still active may be logged by the decorator as:

```text
unexpected failure
```

and then converted by `ProviderPaymentResultResolver` into expected Inquiry recovery.

This is an observability classification mismatch.

It does not currently create duplicate-charge risk, but should be hardened.

---

# 59. Provider Adapter Folder Structure

Recommended per-provider layout:

```text
PayBridge.Modules.Providers.Infrastructure/
├── PayTR/
│   ├── PayTRPaymentProvider.cs
│   ├── PayTRClient.cs
│   ├── PayTROptions.cs
│   ├── PayTRCredentialPayload.cs
│   ├── Requests/
│   ├── Responses/
│   ├── Mapping/
│   └── Security/
│
├── Iyzico/
│   └── ...
│
├── Decorators/
└── PaymentProviderFactory.cs
```

Exact names can vary.

The important rule is provider-specific code stays grouped and isolated.

---

# 60. Adapter vs HTTP Client Responsibility

Recommended separation:

```text
PayTRPaymentProvider
    ↓
interprets PayBridge provider contracts
    ↓
PayTRClient
    ↓
HTTP serialization / headers / endpoint calls
```

This avoids a single adapter class becoming:

- serializer
- HTTP client
- signature generator
- error mapper
- business policy
- retry engine

all at once.

---

# 61. HttpClientFactory Target

Real HTTP-based providers should use:

```text
IHttpClientFactory
```

or typed clients.

Do not create a new raw `HttpClient` for every provider request.

Benefits include:

- connection pooling
- DNS refresh handling
- centralized timeout configuration
- delegating handlers
- observability

Status:

```text
TARGET
```

because current Mock provider performs no HTTP.

---

# 62. Provider Timeout Policy

Each real provider needs an explicit timeout.

Timeout must be shorter than an unbounded request lifetime but long enough for provider SLA.

Timeout must NOT automatically imply retry Charge.

Timeout means:

```text
delivery/result may be uncertain
```

and should feed Inquiry/reconciliation semantics.

---

# 63. Retry Policy Warning

Do NOT apply a generic:

```text
Retry 3 times on transient HTTP errors
```

policy to Charge.

Even common resilience libraries can create double-charge risk when applied without payment-specific delivery semantics.

---

# 64. Inquiry Retry Policy

Inquiry is a status operation and is generally safer to retry than Charge.

Even then define:

- timeout
- attempt limit
- backoff
- provider rate limits
- cancellation
- reconciliation SLA

Current short Inquiry policy is:

```text
0 ms
500 ms
1000 ms
```

inside the request lifecycle.

---

# 65. Circuit Breaker

Future provider integrations may use:

```text
circuit breaker
```

to protect new traffic.

But a circuit breaker does NOT resolve an existing uncertain Payment.

Provider health and payment recovery are different concerns.

---

# 66. Provider Access Tokens

Some PSPs require:

```text
merchant credentials
    ↓
OAuth/token endpoint
    ↓
short-lived provider access token
```

PayBridge currently does not implement this lifecycle.

---

# 67. Target Provider Token Architecture

Future conceptual flow:

```text
MerchantProviderAccount
    ↓
Provider credential
    ↓
Redis provider-token cache
    ↓
cache hit?
    ├── yes → use token
    └── no
         ↓
distributed refresh lock
         ↓
double-check
         ↓
provider auth endpoint
         ↓
cache token with expiry safety buffer
```

Status:

```text
TARGET
```

---

# 68. Provider Token Cache Is Not Financial Truth

Redis provider-token cache would be an authentication optimization.

It must NOT become:

```text
payment state
idempotency truth
provider transaction truth
```

Provider token failure should affect provider connectivity, not redefine financial state.

---

# 69. Distributed Token Refresh Lock

Without a refresh lock, many PayBridge instances can notice the same token expiration and all call the provider token endpoint simultaneously.

Target:

```text
one refresher
others wait/re-read
```

This protects provider auth endpoints from token-refresh stampedes.

---

# 70. Token Expiry Buffer

A cached access token should not be used until its literal last millisecond.

Future token cache should account for:

```text
network latency
clock skew
request duration
```

with a safety buffer.

Exact buffer should be provider-specific/configurable.

---

# 71. Credential Payload Schema

Each provider should define a typed internal model.

Example:

```csharp
internal sealed record PayTRCredentialPayload(
    string MerchantId,
    string MerchantKey,
    string MerchantSalt);
```

Adapter:

```text
CredentialPayloadJson
    ↓
deserialize typed payload
    ↓
validate required fields
```

Do not pass arbitrary dynamic JSON throughout the provider implementation.

---

# 72. Credential Validation

Provider adapter should fail fast before external Charge if required credential fields are missing.

This is a:

```text
configuration/pre-send failure
```

not an uncertain payment result.

It should be clearly classified and logged without exposing secrets.

---

# 73. Credential Deserialization Failure

If JSON cannot deserialize into expected provider credential shape:

```text
do not call Charge
```

Return/throw a provider configuration error before external financial side effect.

This may later be eligible for safe routing fallback because delivery never occurred.

---

# 74. Merchant Provider Account Selection

Current Sale flow already verifies:

```text
provider account exists
active
channel allowed
active credential exists
```

before Charge.

New provider adapters should not duplicate merchant database lookup.

They receive already-resolved runtime credential context.

---

# 75. Adapter Must Not Query Merchant DB

Do not implement:

```text
PayTRPaymentProvider
    ↓
MerchantDbContext
    ↓
load API key
```

That breaks provider modularity and mixes persistence with external communication.

Credential resolution belongs outside adapter.

---

# 76. Adapter Must Not Query Payment DB

Do not implement:

```text
IyzicoPaymentProvider
    ↓
PaymentsDbContext
```

Provider adapter should receive all required operation context through the provider contract.

---

# 77. Current Payment Persistence Limitation

Current `Payment` persists:

```text
ProviderCode
ProviderTransactionId
```

but does not persist:

```text
MerchantProviderAccountId
CredentialId
CredentialVersion
```

Current `PaymentTransaction` similarly persists ProviderCode but not provider-account identity.

---

# 78. Why Provider Account Identity May Matter Later

For short synchronous Sale flow, `ProviderCode` may be sufficient.

For long-lived operations it may become useful to know:

```text
exact merchant provider account used
credential version used
```

especially when:

- credentials rotate
- merchant provider account changes
- reconciliation happens hours later
- refund occurs much later
- multiple provider credentials/history are retained

This requires architectural decision before production reconciliation/refund maturity.

---

# 79. Do Not Persist Raw Credentials in Payment

If provider-account identity is added later, persist:

```text
MerchantProviderAccountId
credential version/reference
```

if needed.

Do NOT persist:

```text
CredentialPayloadJson
API keys
access tokens
secret values
```

inside Payment/PaymentTransaction.

---

# 80. Provider Error Model

Current global provider error enum only includes:

```text
ProviderNotSupported = 120001
```

Runtime provider response errors are currently represented as strings:

```text
ErrorCode
ErrorMessage
```

This is intentionally flexible for provider-native errors.

---

# 81. Provider Error Normalization Target

Future mature model may track:

```text
ProviderCode
NativeErrorCode
NormalizedCategory
Retryability
Terminality
SafeFallback
Message
```

Example normalized categories:

```text
DECLINED
AUTHENTICATION_FAILURE
RATE_LIMITED
CONFIGURATION_ERROR
TRANSPORT_UNKNOWN
PROCESSING
```

Status:

```text
TARGET
```

---

# 82. Avoid Error-Code Leakage Into Core Domain

The Payment domain should not contain:

```text
PayTR code 99
Iyzico code 10051
Provider-specific enums
```

Store/log provider-native code as metadata where needed.

Domain decisions should use normalized semantics.

---

# 83. Provider Native Message Safety

Provider error messages may contain unexpected data.

Before storing/logging a raw provider message:

- review PII risk
- review secret risk
- limit length
- sanitize where needed

Do not assume external provider messages are safe logs.

---

# 84. Provider Registration

Current DI manually registers:

```text
MockPaymentProvider
```

then registers an `IPaymentProvider` factory that wraps Mock in:

```text
LoggingPaymentProviderDecorator
```

Factory receives the decorated provider collection.

---

# 85. Adding a Second Provider — Current DI Pattern

Conceptually:

```text
services.AddScoped<PayTRPaymentProvider>();
services.AddScoped<IyzicoPaymentProvider>();

services.AddScoped<IPaymentProvider>(sp =>
    new LoggingPaymentProviderDecorator(
        sp.GetRequiredService<PayTRPaymentProvider>(),
        logger));

services.AddScoped<IPaymentProvider>(sp =>
    new LoggingPaymentProviderDecorator(
        sp.GetRequiredService<IyzicoPaymentProvider>(),
        logger));
```

Then factory can resolve by ProviderCode.

---

# 86. DI Scalability Target

As provider count grows, manual decorator registration can become repetitive.

Potential future options:

- helper registration method
- Scrutor decoration
- provider registration descriptor
- keyed services

Do not introduce complexity until more providers exist.

The current explicit setup is easy to understand and adequate for Mock.

---

# 87. Duplicate ProviderCode Risk

Current factory:

```text
FirstOrDefault(...)
```

selects the first matching provider.

There is no reviewed startup validation guaranteeing unique registered `ProviderCode`.

If two adapters expose the same ProviderCode:

```text
registration order may determine the winner
```

This is unsafe.

---

# 88. Recommended Startup Provider Validation

Future hardening should fail startup if:

```text
duplicate ProviderCode
```

is registered.

Example invariant:

```text
registered ProviderCodes must be unique
case-insensitively
```

This should be tested.

---

# 89. Provider Options Validation

Every real provider should validate required configuration during startup where possible.

Examples:

```text
BaseUrl
timeout
token endpoint
certificate config
webhook secret configuration
```

Do not discover missing static configuration only during the first production payment.

Merchant-specific credentials remain runtime data.

---

# 90. Environment Separation

Provider configuration must distinguish:

```text
sandbox
production
```

Do not allow production credentials to accidentally call sandbox or sandbox credentials to production.

Environment should be explicit and validated.

---

# 91. Provider SDK Decision

If a PSP provides an SDK, using it is optional.

Before adopting it evaluate:

- maintenance
- .NET version support
- timeout control
- HTTP pipeline visibility
- logging safety
- retry behavior
- exception types
- testability
- idempotency support

An SDK that hides dangerous retries may be worse than a thin HTTP adapter.

---

# 92. Provider Signature Algorithms

If provider authentication requires request signing:

```text
HMAC
hash
RSA
certificate
```

implementation belongs in provider-specific Infrastructure.

Do not place PSP signature code in Payments.Application.

---

# 93. Card Data Boundary

If future PayBridge accepts raw card data, provider adapters may receive sensitive PCI-scoped information.

That significantly changes:

- regulatory scope
- logging rules
- storage rules
- memory handling
- API design
- security review

Current provider contract does not yet model raw card data.

Do not add it casually.

---

# 94. Tokenized Payment Boundary

Where possible, provider/network token references are preferable to storing raw card secrets.

Any future Masterpass/network token/provider token design must be documented separately and must not be placed in generic credential JSON unless it truly represents merchant credentials.

Merchant credential and customer payment instrument are different concepts.

---

# 95. Refund Provider Contract Target

Current Merchant model already has:

```text
AllowRefund
```

and Payment domain supports refund state transitions.

Future provider contract likely needs a normalized refund operation.

Conceptually:

```text
RefundAsync(
    PaymentId,
    ProviderTransactionId,
    Amount,
    Credential,
    IdempotencyKey
)
```

Exact contract is TARGET and must be designed with transaction boundaries first.

---

# 96. Void Provider Contract Target

Void similarly needs:

```text
provider transaction correlation
credential
idempotency
terminal/unknown response semantics
Inquiry/recovery if provider supports it
```

Do not assume Void failure handling is identical to Charge.

---

# 97. Refund/Void External-Call Rule

When implemented:

```text
Refund
Void
```

must also happen outside long-running SQL transactions.

Follow:

```text
durable pending operation
    ↓
provider call
    ↓
durable finalization
```

Read:

```text
docs/architecture/TRANSACTION_BOUNDARIES.md
```

---

# 98. Webhook Provider Contract Target

Real providers may send:

```text
payment status
refund status
chargeback
void status
```

webhooks.

Provider-specific webhook signature verification and payload parsing belong in Providers.Infrastructure.

Normalized event contracts should cross into application logic.

---

# 99. Webhook Signature Rule

Never accept provider webhook state before validating the provider's required signature/authentication mechanism.

Provider-specific verification belongs in adapter/security infrastructure.

---

# 100. Webhook/Inquiry Convergence

Future system may resolve a Processing payment through:

```text
Inquiry
or
Webhook
```

Both paths must converge on the same domain transition rules.

Provider adapter must normalize both into consistent semantics.

---

# 101. Observability Requirements

Every provider integration should expose safe operational dimensions:

```text
ProviderCode
Operation
PaymentId
State
Duration
NormalizedError
NativeErrorCode if safe
Attempt type
Inquiry attempt
```

Future metrics:

```text
charge success rate
charge failure rate
timeout rate
Inquiry rate
unresolved rate
latency p50/p95/p99
auth/token failure rate
rate-limit rate
```

---

# 102. Do Not Use Logs as Provider State

Logs are observability.

Durable truth is:

```text
Payment
PaymentTransaction
provider
```

Do not build recovery logic by parsing Elasticsearch logs.

---

# 103. Testability Requirement

Provider adapters should be testable without sending real money.

Recommended layers:

```text
adapter unit tests
HTTP client/mapping tests
sandbox integration tests
PayBridge end-to-end tests
```

Mock provider remains useful for generic orchestration tests.

---

# 104. Contract Tests

Every provider adapter should pass shared contract tests.

Examples:

```text
ProviderCode not empty
Charge success returns transaction id
explicit failure maps to Failed
processing maps to StillProcessing
Inquiry never creates Charge
cancellation propagated
credentials not logged
```

---

# 105. Provider-Specific Tests

Each provider also needs tests for:

```text
signature generation
request serialization
response mapping
provider-native error mapping
timestamp/nonce rules
token acquisition
timeout exception mapping
Inquiry correlation
refund/void mapping
webhook verification
```

as applicable.

---

# 106. Charge-Once Invariant Test

For every real provider integration:

```text
simulate uncertain Charge result
```

and assert:

```text
Charge call count = 1
```

Recovery must use:

```text
Inquiry
```

not another Charge.

---

# 107. Inquiry Credential Test

Before production:

```text
Charge request has credential
Inquiry request has required credential
```

must be verified.

This specifically closes the current repository gap where short Inquiry credential is null.

---

# 108. Provider Timeout Test

Do not only return:

```text
StillProcessing
```

from a fake adapter.

Also test actual exceptions:

```text
TimeoutException
TaskCanceledException
HttpRequestException / provider SDK equivalent
```

and define how each maps to PayBridge recovery.

---

# 109. Provider Sandbox Test

Before production activation:

```text
real provider sandbox
```

should verify:

- credentials
- signing
- Charge
- duplicate provider idempotency behavior
- Inquiry
- failed payment
- timeout-like behavior if simulatable
- transaction id
- refund
- void
- webhook

depending on supported capabilities.

---

# 110. Merchant Onboarding Gap

Current API controllers only expose:

```text
IntegrationTokens
Payments
```

There is no reviewed complete merchant/provider-account credential onboarding API in the current controller set.

Therefore provider-account setup is not yet a mature self-service/admin product flow.

Status:

```text
domain/persistence model: IMPLEMENTED
operational onboarding workflow: TARGET/PARTIAL
```

---

# 111. Provider Account Administration Target

Future merchant/admin tooling should support:

```text
create provider account
configure ProviderCode
configure channel permissions
configure refund permission
set priority
activate/deactivate
store/rotate/revoke credentials
test connectivity safely
```

This should be authorization- and audit-protected.

---

# 112. Credential Validation During Onboarding

A future "test credentials" operation must be designed safely.

Do not perform a real financial Charge to validate credentials.

Prefer provider-supported:

```text
auth validation
account info
test endpoint
sandbox
```

where available.

---

# 113. Audit Requirements

Provider configuration changes are financially sensitive.

Future audit should capture:

```text
who changed provider account
when
provider code
activation/deactivation
channel permission changes
priority changes
credential rotation/revoke event
```

Never store secret values in the audit record.

---

# 114. Production Credential Versioning Concern

Current runtime `ProviderCredentialContext` contains:

```text
MerchantProviderAccountId
ProviderCode
CredentialPayloadJson
```

but not:

```text
CredentialId
EncryptionKeyVersion
```

If long-running reconciliation needs exact credential-history identity, current context/persistence may be insufficient.

This is a future design decision.

---

# 115. Credential Rotation During Processing

Scenario:

```text
Charge uses credential A
Payment becomes Processing
merchant rotates to credential B
background Inquiry happens later
```

Questions to define:

```text
Can Inquiry use B for transaction created under A?
Does provider account identity remain the same?
Does provider require original terminal/API key?
```

Provider-specific semantics matter.

---

# 116. Provider Account ID Persistence Option

Future production hardening may persist:

```text
MerchantProviderAccountId
```

on Payment or PaymentTransaction.

Benefit:

```text
exact account correlation
```

without storing secrets.

This is a TARGET design choice, not current implementation.

---

# 117. Provider Credential ID/Version Persistence Option

Only if required by provider semantics, future model may record a non-secret reference such as:

```text
CredentialId
CredentialVersion
```

This improves reconciliation/audit after rotation.

Do not add it unless real provider requirements justify it.

---

# 118. Provider Priority and Smart Routing

Future routing can use:

```text
Priority
provider health
success rate
cost
currency
channel
amount
merchant preferences
```

But routing policy belongs outside individual provider adapters.

Adapters should not decide whether they should have been selected.

---

# 119. Provider Health Data

Health may eventually come from:

```text
recent timeout rate
recent success rate
circuit state
latency
provider incidents
```

This should affect NEW routing.

It must not change recovery provider for an already-sent uncertain payment.

---

# 120. Provider Fallback Safety

Fallback can potentially occur only when the first provider has definitely NOT created the financial side effect.

Safe examples may include:

```text
provider not registered
merchant account inactive
credential invalid before send
local circuit open before send
```

Provider-specific terminal decline fallback is a separate product policy.

Uncertain post-send outcomes must not fallback immediately.

---

# 121. Provider Integration Checklist — Architecture

Before implementing code:

```text
[ ] Stable ProviderCode chosen
[ ] MerchantProviderAccount semantics defined
[ ] Supported channels defined
[ ] Refund support defined
[ ] Credential payload schema defined
[ ] Credential protection strategy defined
[ ] Charge correlation strategy defined
[ ] Inquiry correlation strategy defined
[ ] Timeout semantics understood
[ ] Provider idempotency semantics understood
[ ] Error-state mapping defined
[ ] Cancellation semantics understood
[ ] Rate limits documented
[ ] Access token lifecycle documented if needed
[ ] Webhook semantics documented if available
```

---

# 122. Provider Integration Checklist — Code

```text
[ ] Provider folder created
[ ] IPaymentProvider implementation created
[ ] ProviderCode implemented
[ ] typed credential model created
[ ] typed provider HTTP DTOs created
[ ] mapping isolated
[ ] HttpClient/typed client registered
[ ] timeout configured
[ ] no generic Charge retry
[ ] Inquiry implemented
[ ] credentials propagated to Inquiry
[ ] logging decorator applied
[ ] secrets excluded from logs
[ ] DI registered
[ ] duplicate ProviderCode startup protection considered
```

---

# 123. Provider Integration Checklist — Testing

```text
[ ] happy Charge success
[ ] explicit Charge failure
[ ] processing result
[ ] missing provider transaction id
[ ] real timeout exception
[ ] caller cancellation
[ ] uncertain transport failure
[ ] Inquiry success
[ ] Inquiry failure
[ ] Inquiry unresolved
[ ] Inquiry authentication/credential
[ ] Charge exactly once
[ ] provider idempotency key stable
[ ] secret not logged
[ ] invalid credential payload fails pre-send
[ ] sandbox test complete
```

---

# 124. Provider Integration Checklist — Production

```text
[ ] production BaseUrl validated
[ ] production credentials provisioned securely
[ ] DataProtection/secret key persistence verified
[ ] provider token cache ready if required
[ ] reconciliation strategy ready
[ ] alerts/metrics ready
[ ] rate limits understood
[ ] support/runbook ready
[ ] webhook public endpoint security ready if used
[ ] refund/void support intentionally enabled/disabled
[ ] merchant provider account configured
[ ] rollout/rollback plan documented
```

---

# 125. Recommended New Provider Sequence

Canonical development order:

```text
1. Research PSP API contract.
2. Define ProviderCode.
3. Define typed credential payload.
4. Configure MerchantProviderAccount.
5. Store protected credential.
6. Build provider HTTP client.
7. Build Charge mapping.
8. Build Inquiry mapping.
9. Define failure/unknown mapping.
10. Register adapter + logging decorator.
11. Add resolver/unit tests.
12. Add sandbox integration tests.
13. Verify Charge-once invariant.
14. Verify Inquiry recovery.
15. Verify observability.
16. Only then enable for merchants.
```

---

# 126. Example Provider Adapter Skeleton

Conceptual only:

```csharp
internal sealed class ExamplePaymentProvider
    : IPaymentProvider
{
    public string ProviderCode => "EXAMPLE";

    public async Task<ProviderChargeResponse> ChargeAsync(
        ProviderChargeRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Parse typed credential
        // 2. Build provider-native request
        // 3. Call provider once
        // 4. Map provider-native response
        // 5. Preserve uncertainty
    }

    public async Task<ProviderInquiryResponse> InquiryAsync(
        ProviderInquiryRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Authenticate
        // 2. Query existing financial operation
        // 3. Never call Charge
        // 4. Map status
    }
}
```

This is a structural example, not production implementation.

---

# 127. Adapter Must Preserve Cancellation

Provider HTTP calls should accept and propagate:

```text
CancellationToken
```

Do not swallow caller cancellation.

The recovery layer distinguishes caller cancellation from provider timeout.

---

# 128. Adapter Timeout vs Caller Cancellation

If the provider HTTP timeout occurs but caller token is not cancelled:

```text
timeout
→ uncertain/recovery classification
```

If caller token is cancelled:

```text
cancellation propagates
```

A real HTTP client implementation must preserve enough distinction for `ProviderPaymentResultResolver`.

---

# 129. Avoid Task.Run Around Provider I/O

Provider HTTP communication is naturally asynchronous.

Do not wrap:

```text
ChargeAsync
InquiryAsync
```

in:

```text
Task.Run(...)
```

That does not make network I/O more asynchronous and wastes thread-pool resources.

---

# 130. Provider Adapter Lifetime

Current providers are registered:

```text
Scoped
```

A real adapter should generally remain stateless with per-request data passed through method arguments.

Do not store merchant credentials in instance fields that can leak between operations.

---

# 131. HttpClient Lifetime

If typed HttpClient is introduced, let `IHttpClientFactory` manage underlying handlers/connections.

Do not put per-merchant credential into global default headers on a shared client if those values can leak across requests.

Build request-specific authorization safely.

---

# 132. Concurrency Safety

Provider adapter implementations should not rely on mutable shared state per payment.

PayBridge may process multiple merchant payments concurrently.

Avoid:

```text
_currentMerchantApiKey
_currentPaymentId
_currentAccessTokenForRequest
```

as mutable singleton/shared fields.

---

# 133. Provider Token Cache Key Target

If provider access-token caching is implemented, a future Redis key should include enough identity to prevent token sharing between different merchant accounts.

Conceptually:

```text
provider-token:{providerCode}:{merchantProviderAccountId}
```

potentially also environment/token-scope dimension if required.

Exact format must be documented before implementation.

---

# 134. No Token Cross-Tenant Leakage

Never cache a provider access token only by:

```text
providerCode
```

if credentials/tokens are merchant-specific.

That could cause one merchant's token to be used for another merchant.

---

# 135. Credential Payload Validation Target

Each provider integration should validate:

```text
credential schema version
required keys
field lengths/formats
environment compatibility
```

before Charge.

This keeps configuration errors out of remote financial execution.

---

# 136. Provider Configuration Versioning

As providers evolve their credential requirements, a future payload schema version may be useful.

Example conceptual:

```json
{
  "schemaVersion": 1,
  "merchantId": "...",
  "apiKey": "..."
}
```

Status:

```text
TARGET if provider credential evolution requires it
```

---

# 137. Provider Contract Package Observation

`Providers.Contracts` currently references:

```text
Microsoft.AspNetCore.DataProtection.Abstractions
```

even though the reviewed core provider contract types do not require DataProtection directly.

This may be unnecessary package coupling.

It is not a runtime blocker, but can be reviewed/cleaned during architecture hardening.

---

# 138. Package Version Consistency

Provider projects currently use a mixture of Microsoft.Extensions package major versions.

Before introducing real provider HTTP dependencies, align package versions with the solution's target framework/package strategy to avoid subtle dependency-resolution issues.

This is maintenance hardening rather than provider-flow behavior.

---

# 139. Current Merchant Onboarding Limitation

Merchant Application currently does not expose a mature set of provider-account administration use cases in the reviewed project structure.

Therefore a real PSP integration needs two different things:

```text
technical adapter
+
operational merchant onboarding/configuration
```

The adapter alone does not make the provider usable by merchants.

---

# 140. Separation of Provider Availability

Three levels should be distinguished:

```text
Adapter registered globally
    ↓
MerchantProviderAccount exists + active
    ↓
Channel/refund capability enabled + active credential
```

Only then can a merchant actually use the provider.

---

# 141. Provider Disable Behavior

Global adapter removal:

```text
factory cannot resolve ProviderCode
```

Merchant account deactivation:

```text
credential resolver cannot return active account
```

These are different operational controls.

Provider incident management may eventually need a global provider enable/disable/health layer separate from merchant configuration.

---

# 142. Global Provider Configuration Target

Future provider registry may include:

```text
ProviderCode
DisplayName
IsGloballyEnabled
SupportedOperations
SupportedChannels
BaseUrl/environment
health state
```

MerchantProviderAccount then represents merchant-specific enablement.

Status:

```text
TARGET
```

---

# 143. Provider Feature Capability Target

Different PSPs may support different operations.

Future capability metadata may include:

```text
Charge
Inquiry
Refund
PartialRefund
Void
Webhook
3DS
TokenizedPayment
Installment
```

Do not assume all `IPaymentProvider` implementations support every future operation.

Interface design may need capability separation rather than one giant provider interface.

---

# 144. Interface Segregation Target

As operations grow, consider:

```text
IPaymentChargeProvider
IPaymentInquiryProvider
IPaymentRefundProvider
IPaymentVoidProvider
```

or capability contracts.

Do not turn `IPaymentProvider` into a huge interface every adapter must fake.

This is a future design decision.

---

# 145. Current No-Second-Charge ADR Dependency

Every provider adapter must obey the system-wide invariant:

```text
uncertain Charge
→ Inquiry/reconciliation
```

not:

```text
uncertain Charge
→ second Charge
```

A dedicated ADR should formalize this:

```text
ADR-003-NO-SECOND-CHARGE.md
```

---

# 146. Provider Integration and Idempotency

Two idempotency layers exist:

```text
PayBridge API idempotency
    ↓
prevents duplicate workflow ownership

Provider idempotency key
    ↓
provider-side defense in depth
```

A provider adapter must not change PayBridge API idempotency semantics.

Read:

```text
docs/flows/IDEMPOTENCY_FLOW.md
```

---

# 147. Provider Integration and Transactions

Provider external I/O must remain outside:

```text
CreatePayment transaction
CompletePayment transaction
```

The adapter must not open PayBridge DB transactions.

Read:

```text
docs/architecture/TRANSACTION_BOUNDARIES.md
```

---

# 148. Provider Integration and Inquiry

The adapter's Inquiry implementation is part of payment safety, not an optional reporting endpoint.

Read:

```text
docs/flows/PROVIDER_INQUIRY_FLOW.md
```

before implementing any real provider.

---

# 149. Provider Integration and Refund

The current Refund implementation is incomplete.

Do not add provider Refund directly into the existing transactional `RefundPaymentCommandHandler`.

Refund must first be restructured around:

```text
local pending transaction
provider call outside DB transaction
local completion transaction
```

---

# 150. Current Production-Readiness Gaps

Before PayBridge can onboard a real production PSP confidently, address at least:

```text
1. Provider transport-exception classification.
2. Provider recovery tests.
3. Production credential key persistence.
4. Real merchant provider onboarding workflow.
5. Provider HTTP client/timeout standard.
6. Duplicate ProviderCode startup validation.
7. Provider account/credential audit.
8. Persistent reconciliation.
9. Credential-rotation behavior for Processing payments.
10. Refund/Void provider contract design.
11. Production observability/metrics.
```

---

# 151. Current Design Strengths

The repository already provides a solid provider foundation:

```text
✓ provider-specific implementation separated from Payments
✓ small provider contract
✓ explicit Charge / Inquiry distinction
✓ provider factory
✓ logging decorator
✓ merchant-specific provider accounts
✓ channel capability flags
✓ refund capability flag
✓ protected credential payload model
✓ one active credential per provider account
✓ credential rotation/revoke domain methods
✓ credential reader uses contracts/projection
✓ resolved credentials passed to Charge
✓ provider transaction id persisted
✓ provider code persisted
✓ no blind second Charge in resolver
```

These should be preserved.

---

# 152. Agent Guardrails

Before modifying provider integration, answer:

```text
[ ] Is this provider-specific logic?
[ ] Does it belong in Providers.Infrastructure?
[ ] Does Payments need to know this provider-native field?
[ ] Is ProviderCode stable/canonical?
[ ] Is merchant provider account active?
[ ] Is channel supported?
[ ] Is credential active?
[ ] Is credential payload being logged?
[ ] Is Charge called exactly once?
[ ] Can uncertain Charge be queried?
[ ] Does Inquiry receive credentials?
[ ] Is ProviderTransactionId available?
[ ] Does this add generic retry to Charge?
[ ] Is provider I/O outside SQL transaction?
[ ] Are cancellation and timeout distinct?
[ ] Does this affect smart-routing safety?
[ ] Does this require reconciliation support?
```

---

# 153. Forbidden Change — Provider Logic in PaymentOrchestrator

Do not:

```csharp
if (providerCode == "PAYTR")
{
    ...
}
```

inside `PaymentOrchestrator`.

---

# 154. Forbidden Change — Raw Credential Logging

Do not:

```csharp
_logger.LogInformation(
    "Credential {Credential}",
    request.Credential);
```

---

# 155. Forbidden Change — Provider DB Access

Do not inject PayBridge DbContexts into provider adapters.

---

# 156. Forbidden Change — Generic Financial Retry

Do not configure automatic Charge retry on:

```text
timeout
HTTP 500
connection reset
TaskCanceledException
```

without explicit delivery/idempotency guarantees.

---

# 157. Forbidden Change — New Provider Code Per Request

ProviderCode is a stable provider identity.

Do not generate dynamic codes such as:

```text
PAYTR-MERCHANT-123
```

Merchant identity belongs to MerchantProviderAccount, not provider type identity.

---

# 158. Forbidden Change — Secret in Payment Aggregate

Do not add:

```text
ProviderApiKey
ProviderSecret
ProviderAccessToken
```

to `Payment` or `PaymentTransaction`.

---

# 159. Forbidden Change — Inquiry as Charge Retry

`InquiryAsync` must never create a new payment operation.

---

# 160. Forbidden Change — Smart Routing Inside Adapter

Adapter implements communication with one PSP.

It should not decide to call another PSP.

Routing belongs above the adapter layer.

---

# 161. Compact Current Integration Flow

```text
Payment Request
    ↓
ProviderCode
    ↓
PaymentProviderFactory
    ↓
LoggingPaymentProviderDecorator
    ↓
Provider Adapter

Before Charge:
MerchantId + ProviderCode + Channel
    ↓
MerchantProviderAccountReader
    ↓
Active Account
    ↓
Active Credential
    ↓
CredentialProtector.Unprotect
    ↓
ProviderCredentialContext

Then:
ProviderChargeRequest
    ↓
Adapter.ChargeAsync
    ↓
ProviderChargeResponse
    ↓
ProviderPaymentResultResolver
    ↓
optional Adapter.InquiryAsync
    ↓
ProviderFinalResult
    ↓
CompletePayment
```

---

# 162. Compact Target Real-PSP Flow

```text
Merchant Provider Account
        ↓
Encrypted Credential
        ↓
Resolve + Decrypt
        ↓
Provider adapter
        ↓
Provider token cache if required
        ↓
Typed HTTP client
        ↓
Charge exactly once
        ↓
Normalize PSP response
        ↓
Success / Failed / Uncertain
                    ↓
                 Inquiry
                    ↓
         Success / Failed / Processing
                    ↓
           Persistent reconciliation
```

---

# 163. Recommended First Real PSP Definition of Done

A real PSP integration is NOT done when Charge returns 200.

Definition of done:

```text
✓ adapter isolated
✓ credential schema defined
✓ credential securely stored
✓ Charge mapped
✓ provider idempotency understood
✓ Inquiry implemented
✓ Inquiry works after lost Charge response
✓ transaction id mapping correct
✓ timeout classification correct
✓ caller cancellation tested
✓ Charge exactly once test passes
✓ provider-native errors normalized
✓ logging sanitized
✓ metrics visible
✓ sandbox integration passes
✓ merchant onboarding/config exists
✓ Processing recovery strategy exists
✓ refund/void capability explicitly documented
```

---

# 164. Documentation Update Triggers

Update this file whenever any of these change:

```text
IPaymentProvider
IPaymentProviderFactory
ProviderCode semantics
provider DI registration
provider folder structure
ProviderChargeRequest
ProviderInquiryRequest
ProviderPaymentState
MerchantProviderAccount
MerchantProviderCredential
credential encryption
provider-account lookup
provider access-token cache
real PSP adapter
routing
fallback
refund
void
webhook
reconciliation
provider logging
```

Major provider contract changes should receive an ADR.

---

# 165. Related Documentation

Read with:

```text
/AGENTS.md

/docs/architecture/SYSTEM_ARCHITECTURE.md
/docs/architecture/TRANSACTION_BOUNDARIES.md

/docs/flows/PAYMENT_EXECUTION_FLOW.md
/docs/flows/IDEMPOTENCY_FLOW.md
/docs/flows/PROVIDER_INQUIRY_FLOW.md
/docs/flows/TOKEN_FLOW.md
```

Future related files:

```text
/docs/flows/REFUND_VOID_FLOW.md
/docs/flows/WEBHOOK_FLOW.md

/docs/infrastructure/REDIS.md
/docs/infrastructure/OBSERVABILITY.md

/docs/decisions/ADR-003-NO-SECOND-CHARGE.md
/docs/decisions/ADR-005-PROVIDER-ABSTRACTION.md
```

---

# 166. Final Agent Rule

A PayBridge provider integration has two responsibilities:

```text
1. Communicate correctly with one PSP.
2. Preserve PayBridge's financial safety semantics.
```

The adapter must translate provider-specific behavior into PayBridge's normalized contract without inventing certainty.

The canonical rule is:

```text
Provider-specific details stay inside the adapter.

Merchant credentials are resolved before the adapter.

Charge is sent once.

Uncertain outcome is preserved.

Inquiry resolves uncertainty when possible.

Processing remains recoverable.

Secrets never enter logs or payment aggregates.

Routing selects providers; adapters do not route.
```

A provider that can Charge but cannot be safely recovered after uncertainty is not a complete PayBridge integration.
