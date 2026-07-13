using MediatR;
using PayBridge.BuildingBlocks.Persistence.Idempotency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PayBridge.BuildingBlocks.CQRS.Behaviors
{
    public sealed class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
        where TRequest : ICommand<TResponse>, IIdempotentCommand<TResponse>
        where TResponse : class
    {
        private readonly IServiceProvider _serviceProvider;
        public IdempotencyBehavior(IServiceProvider provider)
        {
            _serviceProvider = provider;   
        }
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is not IIdempotentCommand<TResponse>)
            {
                return await next();
            }
            var idempotencyService = (IIdempotencyService?)_serviceProvider.GetService(typeof(IIdempotencyService));
            if (idempotencyService is null)
            {
                return await next();
            }
            var properties = request.GetType().GetProperties()
                .Select(p => p.GetValue(request))
                .Where(v => v != null)
                .ToArray();
            string prefix = request.GetType().Name.Replace("Command", "").ToLower();
            string idempotencyKey = IdempotencyKeyGenerator.GenerateIdempotencyKey(prefix, properties);

            var checkResult = await idempotencyService.GetInFlightOrCompletedResultAsync(idempotencyKey, cancellationToken);
            if (checkResult is not null && checkResult != "InFlight_Handled")
            {
                // Eğer daha önce başarıyla tamamlanmışsa veritabanındaki eski cevabı deserialize edip doğrudan dönüyoruz!
                return JsonSerializer.Deserialize<TResponse>(checkResult)!;
            }
            await idempotencyService.CreateInFlightAsync(idempotencyKey, cancellationToken);

            var response = await next();

            await idempotencyService.CompleteAsync(idempotencyKey, response!, cancellationToken);

            return response;
        }
    }
}
