using MediatR;
using PayBridge.BuildingBlocks.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.BuildingBlocks.CQRS.Behaviors
{
    public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly IUnitOfWorkResolver _unitOfWorkResolver;
        public TransactionBehavior(IUnitOfWorkResolver unitOfWorkResolver)
        {
            _unitOfWorkResolver = unitOfWorkResolver;
        }
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is not ICommand<TResponse>)
            {
                return await next();
            }
            var unitOfWork  = _unitOfWorkResolver.Resolve(typeof(TRequest));


            if (unitOfWork is null)
            {
                throw new InvalidOperationException(
                    $"UnitOfWork could not be resolved for request type '{typeof(TRequest).FullName}'.");
            }

            await unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var response = await next();
                
                await unitOfWork.SaveChangesAsync(cancellationToken);

                await unitOfWork.CommitTransactionAsync(cancellationToken);

                return response;
            }
            catch 
            {

                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
    }
}
