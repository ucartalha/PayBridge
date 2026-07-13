using System.Text.Json;

namespace PayBridge.Api.Errors
{
    public sealed class ErrorCatalog : IErrorCatalog
    {
        private readonly Dictionary<int, ErrorDescriptor> _errors;
        public ErrorCatalog(IWebHostEnvironment environment)
        {
            var filePath = Path.Combine(
                environment.ContentRootPath,
                "Errors",
                "errors.tr.json"
                );
            if (!File.Exists(filePath))
            {
                throw new InvalidOperationException($"Error Catalog filed not found : {filePath}");
            }

            var json = File.ReadAllText(filePath);

            var errors = JsonSerializer.Deserialize<List<ErrorDescriptor>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            if (errors is null)
            {
                throw new InvalidOperationException("Error catalog could not be serialized");
            }
            _errors = errors.ToDictionary(x => x.Code);
        }

        public ErrorDescriptor GetByCode(int errorCode)
        {
            if (_errors.TryGetValue(errorCode, out var descriptor))
            {
                return descriptor;
            }
            return new ErrorDescriptor
            {
                Code = errorCode,
                Key = "system.unknown_error_code",
                Message = $"Unknown error code: {errorCode}",
                UserMessage = "İşlem sırasında beklenmeyen bir hata oluştu."
            };
        }

    }
}

