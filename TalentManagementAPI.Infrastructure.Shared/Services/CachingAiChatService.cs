#nullable enable
using System.Security.Cryptography;
using System.Text;
using TalentManagementAPI.Application.Interfaces;
using TalentManagementAPI.Application.Interfaces.Caching;

namespace TalentManagementAPI.Infrastructure.Shared.Services
{
    /// <summary>
    /// Decorator for IAiChatService that adds cache-aside behaviour.
    /// Checks the cache first; on a miss, delegates to the inner service and stores the result.
    /// Sets IAiResponseMetadata.WasCacheHit so controllers can emit X-AI-Cache headers.
    /// </summary>
    public sealed class CachingAiChatService : IAiChatService
    {
        private readonly IAiChatService _inner;
        private readonly ICacheProvider _cache;
        private readonly IAiResponseMetadata _metadata;
        private readonly TimeSpan _ttl;

        public CachingAiChatService(
            IAiChatService inner,
            ICacheProvider cache,
            IAiResponseMetadata metadata,
            TimeSpan ttl)
        {
            _inner = inner;
            _cache = cache;
            _metadata = metadata;
            _ttl = ttl;
        }

        public async Task<string> ChatAsync(
            string message,
            string? systemPrompt = null,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = BuildCacheKey(message, systemPrompt);

            var cached = await _cache.GetAsync<string>(cacheKey, cancellationToken).ConfigureAwait(false);
            if (cached is not null)
            {
                _metadata.WasCacheHit = true;
                return cached;
            }

            var reply = await _inner.ChatAsync(message, systemPrompt, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(reply))
            {
                await _cache.SetAsync(
                    cacheKey,
                    reply,
                    new CacheEntryOptions(_ttl),
                    cancellationToken).ConfigureAwait(false);
            }

            _metadata.WasCacheHit = false;
            return reply;
        }

        /// <summary>
        /// Builds a deterministic 64-char hex cache key from the prompt inputs.
        /// SHA256 keeps keys compact regardless of how long the system prompt is.
        /// </summary>
        private static string BuildCacheKey(string message, string? systemPrompt)
        {
            var raw = $"{systemPrompt ?? string.Empty}|{message}";
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            return $"ai:chat:{Convert.ToHexString(hash).ToLowerInvariant()}";
        }
    }
}
