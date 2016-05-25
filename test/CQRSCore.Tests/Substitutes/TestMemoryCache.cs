using Microsoft.Extensions.Caching.Memory;

namespace CQRSCore.Tests.Substitutes
{
    public class TestMemoryCache : MemoryCache
    {
        public TestMemoryCache() : base(new MemoryCacheOptions())
        {
        }
    }
}
