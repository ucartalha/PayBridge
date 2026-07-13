using System.Security.Cryptography;
using System.Text;

namespace PayBridge.BuildingBlocks.Persistence.Idempotency
{
    public static class IdempotencyKeyGenerator
    {
        public static string GenerateIdempotencyKey(string prefix, params object[] components)
        {
            if (components == null || components.Length == 0)
            {
                throw new ArgumentException("Idempotency key bileşenleri boş olamaz", nameof(components));
            }

            var rawBuilder = new StringBuilder();
            for (int i = 0; i < components.Length; i++)
            {
                var value = components[i] is IFormattable formattable ?
                    formattable.ToString(null, System.Globalization.CultureInfo.InvariantCulture) :
                    components[i]?.ToString();

                rawBuilder.Append(value);
                if (i < components.Length - 1)
                {
                    rawBuilder.Append(":");
                }
            }

            byte[] inputBytes = Encoding.UTF8.GetBytes(rawBuilder.ToString());
            byte[] hashBytes = SHA256.HashData(inputBytes);

            var hashBuilder = new StringBuilder(hashBytes.Length * 2);
            foreach (byte b in hashBytes)
            {
                hashBuilder.Append(b.ToString("x2"));
            }
            return string.IsNullOrWhiteSpace(prefix) ? hashBuilder.ToString() : $"{prefix}:{hashBuilder}";
        }
    }
}
