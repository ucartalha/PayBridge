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

    public async Task<IdempotencyStoreResult>
    TryAcquireOrGetAsync(
        string key,
        CancellationToken cancellationToken)
    {
        var candidate =
            IdempotencyRecord.CreateInFlight(key);

        await _repository.AddAsync(
            candidate,
            cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return IdempotencyStoreResult.Acquired();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlException &&
                  sqlException.Number is 2601 or 2627)
        {
            _dbContext.Entry(candidate).State =
                EntityState.Detached;

            var existingRecord =
                await _dbContext.IdempotencyRecords
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        x => x.IdempotencyKey == key,
                        cancellationToken);

            if (existingRecord is null)
            {
                throw;
            }

            if (existingRecord.Status == "Completed" &&
                !string.IsNullOrWhiteSpace(
                    existingRecord.ResponseContent))
            {
                return IdempotencyStoreResult.Completed(
                    existingRecord.ResponseContent);
            }

            return IdempotencyStoreResult.InFlight();
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
            throw new InvalidOperationException(
         $"Idempotency record was not found while completing key '{key}'.");
        }

        record.Complete(result);

        // FirstOrDefaultAsync tracked entity döndürdüğü için
        // ayrıca Update(record) çağırmaya gerek yok.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}