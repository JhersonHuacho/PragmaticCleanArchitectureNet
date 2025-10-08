using Bookify.Application.Abstractions.Caching;
using Bookify.Domain.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Abstractions.Behaviors;

internal sealed class QueryCachingBehavior<TRequest, TResponse>
	: IPipelineBehavior<TRequest, TResponse>
	where TRequest : ICachedQuery
	where TResponse : Result
{
	private readonly ICacheService _cacheService;
	private readonly ILogger<QueryCachingBehavior<TRequest, TResponse>> _logger;

	public QueryCachingBehavior(ICacheService cacheService, ILogger<QueryCachingBehavior<TRequest, TResponse>> logger)
	{
		_cacheService = cacheService;
		_logger = logger;
	}

	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		TResponse? cachedResult = await _cacheService.GetAsync<TResponse>(request.CacheKey, cancellationToken);

		string name = request.GetType().Name;
		if (cachedResult is not null)
		{
			_logger.LogInformation("Cache hit for request {Query} with key {CacheKey}", name, request.CacheKey);
			return cachedResult;
		}

		_logger.LogInformation("Cache miss for {Query}", name);

		var result = await next();

		if (result.IsSuccess)
		{
			await _cacheService.SetAsync(request.CacheKey, result, request.Expiration, cancellationToken);
			_logger.LogInformation("Cached result for request {Query} with key {CacheKey}", name, request.CacheKey);
		}
		else
		{
			_logger.LogInformation("Not caching failed request {Query}", name);
		}

		return result;
	}
}
