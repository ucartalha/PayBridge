using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PayBridge.BuildingBlocks.Exceptions;
using PayBridge.BuildingBlocks.Persistence;
using PayBridge.BuildingBlocks.Persistence.Idempotency;
using PayBridge.Modules.Payments.Domain.Payments.Entities;
using PayBridge.Modules.Payments.Domain.Payments.Errors;

namespace PayBridge.Modules.Payments.Infrastructure.Persistence.Idempotency;

internal sealed class PaymentsIdempotencyService : IIdempotencyService
{
    private readonly IRepository<IdempotencyRecord> _repository;
    private readonly PaymentsUnitOfWork _unitOfWork;
    private readonly PaymentsDbContext _dbContext;

    public PaymentsIdempotencyService(
        IRepository<IdempotencyRecord> repository,
        PaymentsUnitOfWork unitOfWork,
        PaymentsDbContext dbContext)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
    }

    public async Task<string?> TryAcquireOrGetCompletedResultAsync(
        string key,
        CancellationToken cancellationToken)
    {
        var candidate = IdempotencyRecord.CreateInFlight(key);

        await _repository.AddAsync(
            candidate,
            cancellationToken);

        try
        {
            // İlk request key'in sahibi olmaya çalışıyor.
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // INSERT başarılıysa bu request owner.
            return null;
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlException &&
                  sqlException.Number is 2601 or 2627)
        {
            // Başarısız INSERT entity'si ChangeTracker'da Added kalmasın.
            _dbContext.Entry(candidate).State = EntityState.Detached;

            var existingRecord =
                await _dbContext.IdempotencyRecords
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        x => x.IdempotencyKey == key,
                        cancellationToken);

            // PK hatası aldık ama kayıt artık yoksa bu normal
            // duplicate senaryosu değildir.
            if (existingRecord is null)
            {
                throw;
            }

            // Önceki işlem tamamen bittiyse final response'u replay et.
            if (existingRecord.Status == "Completed" &&
                !string.IsNullOrWhiteSpace(existingRecord.ResponseContent))
            {
                return existingRecord.ResponseContent;
            }
                
            // Kayıt mevcut ama işlem halen devam ediyor.
            throw new BusinessException(
                (int)PaymentErrorCode.PaymentAlreadyInProgress,
                ex);
        }
    }

    public async Task CompleteAsync(
        string key,
        object result,
        CancellationToken cancellationToken)
    {
        var record =
            await _repository.FirstOrDefaultAsync(
                x => x.IdempotencyKey == key,
                cancellationToken);

        if (record is null)
        {
            return;
        }

        record.Complete(result);

        // FirstOrDefaultAsync tracked entity döndürdüğü için
        // ayrıca Update(record) çağırmaya gerek yok.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}