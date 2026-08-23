using MediatR;
using PayBridge.BuildingBlocks.CQRS.Observability;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.BuildingBlocks.CQRS.Behaviors
{
    public sealed class ApmBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    {
        private readonly IApplicationTracer _tracer;

        public ApmBehavior(IApplicationTracer tracer)
        {
            _tracer = tracer;
        }

        public Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            return _tracer.CaptureSpanAsync(
                requestName,
                "cqrs",
                () => next());
        }
    }
}
