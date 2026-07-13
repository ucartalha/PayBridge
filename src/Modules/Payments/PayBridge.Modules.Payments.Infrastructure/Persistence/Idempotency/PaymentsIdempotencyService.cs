using PayBridge.BuildingBlocks.Persistence;
using PayBridge.BuildingBlocks.Persistence.Idempotency;
using PayBridge.Modules.Payments.Domain.Payments.Entities;

namespace PayBridge.Modules.Payments.Infrastructure.Persistence.Idempotency;

internal sealed class PaymentsIdempotencyService : IIdempotencyService
{
    private readonly IRepository<IdempotencyRecord> _repository;
    private readonly PaymentsUnitOfWork _unitOfWork; // Doğrudan modülün kendi UnitOfWork'ünü inject ediyoruz

    public PaymentsIdempotencyService(
        IRepository<IdempotencyRecord> repository,
        PaymentsUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<string?> GetInFlightOrCompletedResultAsync(string key, CancellationToken cancellationToken)
    {
        var record = await _repository.FirstOrDefaultAsync(x => x.IdempotencyKey == key, cancellationToken);
        if (record is null) return null;

        // Domain metodundan ham string sonucunu alıyoruz
        var result = record.CheckStatusAndGetResult();

        // Eğer kayıt Completed ama içeriği henüz boşsa (kıl payı durumlar için) işaretçi dönüyoruz
        return result ?? "InFlight_Handled";
    }

    public async Task CreateInFlightAsync(string key, CancellationToken cancellationToken)
    {
        var record = IdempotencyRecord.CreateInFlight(key);
        await _repository.AddAsync(record, cancellationToken);

        // KRİTİK DÜZELTME 1: InFlight kaydını hemen DB'ye kaydet ki kilitlensin!
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteAsync(string key, object result, CancellationToken cancellationToken)
    {
        var record = await _repository.FirstOrDefaultAsync(x => x.IdempotencyKey == key, cancellationToken);
        if (record is not null)
        {
            record.Complete(result); // DDD: State değişti

            _repository.Update(record);

            // KRİTİK DÜZELTME 2: İşlem bittiğinde Completed durumunu hemen DB'ye push et!
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}