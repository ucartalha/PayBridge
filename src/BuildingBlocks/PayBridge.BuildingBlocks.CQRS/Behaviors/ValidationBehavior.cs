using FluentValidation;
using MediatR;
using PayBridge.BuildingBlocks.Exceptions;
using System.ComponentModel.DataAnnotations;
namespace PayBridge.BuildingBlocks.CQRS.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var validators = _validators.ToList();
            if (!validators.Any())
            {
                return await next();
            }
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))
                );

            var errors = validationResults
            .SelectMany(result => result.Errors)
            .Where(error => error is not null)
            .GroupBy(
                error => error.PropertyName,
                error => error.ErrorMessage)
            .ToDictionary(
                group => group.Key,
                group => group.Distinct().ToArray());
            if (errors.Count !=0)
            {
                throw new PayBridge.BuildingBlocks.Exceptions.ValidationException(errors);
            }

            return await next();
        }
    }
}
