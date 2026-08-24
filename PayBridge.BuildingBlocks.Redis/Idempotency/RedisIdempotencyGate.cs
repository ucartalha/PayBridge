using Microsoft.Extensions.Logging;
using PayBridge.BuildingBlocks.Persistence.Idempotency;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.BuildingBlocks.Redis.Idempotency
{
    public sealed class RedisIdempotencyGate : IIdempotencyGate
    {
        private readonly IDatabase _database;
        private readonly ILogger<RedisIdempotencyGate> _logger;

        private const string ReleaseScript = """
        if redis.call('get', KEYS[1]) == ARGV[1] then
            return redis.call('del', KEYS[1])
        else
            return 0
        end
        """;

        private const string CompleteScript = """
        if redis.call('get', KEYS[1]) == ARGV[1] then
            redis.call('psetex', KEYS[2], ARGV[3], ARGV[2])
            redis.call('del', KEYS[1])
            return 1
        else
            return 0
        end
        """;

        public RedisIdempotencyGate(
            IConnectionMultiplexer connectionMultiplexer,
            ILogger<RedisIdempotencyGate> logger)
        {
            _database = connectionMultiplexer.GetDatabase();
            _logger = logger;
        }

        public async Task<IdempotencyGateResult> TryAcquireOrGetAsync(
            string key,
            TimeSpan inFlightTtl,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var lockKey = BuildLockKey(key);
            var resultKey = BuildResultKey(key);

            try
            {
                // 1. Daha önce tamamlanmış bir response Redis'te varsa
                // doğrudan replay edebiliriz.
                var completedResponse = await _database
                    .StringGetAsync(resultKey)
                    .WaitAsync(cancellationToken);

                if (completedResponse.HasValue)
                {
                    return IdempotencyGateResult.Completed(
                        completedResponse.ToString());
                }

                // 2. Redis lock'un owner token'ı.
                var leaseToken = Guid.NewGuid()
                    .ToString("N");

                // SET key token NX EX
                var acquired = await _database
                    .StringSetAsync(
                        lockKey,
                        leaseToken,
                        inFlightTtl,
                        When.NotExists)
                    .WaitAsync(cancellationToken);

                if (acquired)
                {
                    return IdempotencyGateResult.Acquired(
                        leaseToken);
                }

                // 3. SET NX başarısız olmuş olabilir ama tam bu sırada
                // diğer request işlemi bitirmiş olabilir.
                // Son bir kez completed response kontrol ediyoruz.
                completedResponse = await _database
                    .StringGetAsync(resultKey)
                    .WaitAsync(cancellationToken);

                if (completedResponse.HasValue)
                {
                    return IdempotencyGateResult.Completed(
                        completedResponse.ToString());
                }

                return IdempotencyGateResult.InFlight();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (RedisException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Redis idempotency gate unavailable. Key: {IdempotencyKey}",
                    key);

                return IdempotencyGateResult.Unavailable();
            }
        }

        public async Task MarkCompletedAsync(
            string key,
            string leaseToken,
            string responseContent,
            TimeSpan completedTtl,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var lockKey = BuildLockKey(key);
            var resultKey = BuildResultKey(key);

            try
            {
                var ttlMilliseconds =
                    checked((long)completedTtl.TotalMilliseconds);

                var result = await _database
                    .ScriptEvaluateAsync(
                        CompleteScript,
                        new RedisKey[]
                        {
                        lockKey,
                        resultKey
                        },
                        new RedisValue[]
                        {
                        leaseToken,
                        responseContent,
                        ttlMilliseconds
                        })
                    .WaitAsync(cancellationToken);

                if ((long)result == 0)
                {
                    _logger.LogWarning(
                        "Redis idempotency completion skipped because lease token no longer owns the lock. Key: {IdempotencyKey}",
                        key);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (RedisException ex)
            {
                // Redis cache/gate başarısız olsa bile SQL source of truth
                // etkilenmemeli.
                _logger.LogWarning(
                    ex,
                    "Failed to mark Redis idempotency key as completed. Key: {IdempotencyKey}",
                    key);
            }
        }

        public async Task ReleaseAsync(
            string key,
            string leaseToken,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var lockKey = BuildLockKey(key);

            try
            {
                await _database
                    .ScriptEvaluateAsync(
                        ReleaseScript,
                        new RedisKey[]
                        {
                        lockKey
                        },
                        new RedisValue[]
                        {
                        leaseToken
                        })
                    .WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (RedisException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to release Redis idempotency lock. Key: {IdempotencyKey}",
                    key);
            }
        }

        private static RedisKey BuildLockKey(string key)
        {
            return $"idempotency:lock:{{{key}}}";
        }

        private static RedisKey BuildResultKey(string key)
        {
            return $"idempotency:result:{{{key}}}";
        }
    }
}
