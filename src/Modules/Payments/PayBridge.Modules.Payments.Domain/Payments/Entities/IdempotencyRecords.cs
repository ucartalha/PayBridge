using PayBridge.BuildingBlocks.Exceptions;
using PayBridge.Modules.Payments.Domain.Payments.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PayBridge.Modules.Payments.Domain.Payments.Entities
{
    public sealed class IdempotencyRecord
    {
        public string IdempotencyKey { get; private set; } = null!;
        public string Status { get; private set; } = null!;
        public string? ResponseContent { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }
        private IdempotencyRecord() { }

        public static IdempotencyRecord CreateInFlight(string idempotencyKey)
        {
            return new IdempotencyRecord
            {
                IdempotencyKey = idempotencyKey,
                Status = "InFlight",
                CreatedAtUtc = DateTime.UtcNow
            };
        }
        public string? CheckStatusAndGetResult()
        {

            if (Status == "InFlight")
            {
                throw new BusinessException((int)PaymentErrorCode.OnlySucceededPaymentCanBeVoided); // Mükerrer işlem hatası [cite: 10]
            }

            return ResponseContent;

        }
        public void Complete(object result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            Status = "Completed";
            ResponseContent = JsonSerializer.Serialize(result);
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
