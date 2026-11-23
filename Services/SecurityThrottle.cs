using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;
namespace EduFlow.Web.Services;

// Bộ đếm dùng bộ nhớ có giới hạn; không lưu email/mật khẩu nguyên văn trong khóa.
public sealed class SecurityThrottle : IDisposable
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions { SizeLimit = 20000 });
    private readonly object _gate = new();
    private readonly TimeProvider _time;
    private record Counter(int Count, DateTimeOffset Expires);
    public SecurityThrottle(TimeProvider time) { _time = time; }
    public bool Allow(string group, string identity, int limit, TimeSpan window)
    {
        var key = group + ":" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity.Trim().ToUpperInvariant())));
        lock (_gate)
        {
            var now = _time.GetUtcNow();
            if (!_cache.TryGetValue<Counter>(key, out var value) || value!.Expires <= now)
                value = new Counter(0, now.Add(window));
            if (value.Count >= limit) return false;
            _cache.Set(key, value with { Count = value.Count + 1 }, new MemoryCacheEntryOptions { AbsoluteExpiration = value.Expires, Size = 1 });
            return _cache.TryGetValue(key, out _);
        }
    }
    public void Dispose() => _cache.Dispose();
}
