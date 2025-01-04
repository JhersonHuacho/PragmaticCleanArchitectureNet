using Microsoft.Extensions.Caching.Distributed;

namespace Bookify.Infrastructure.Caching
{
	public static class CacheOptions
	{
		public static DistributedCacheEntryOptions DefaultExpiration => new DistributedCacheEntryOptions
		{
			AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
		};

		public static DistributedCacheEntryOptions Create(TimeSpan? expiration)
		{
			return expiration is not null
				? new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration }
				: DefaultExpiration;
		}
	}
}