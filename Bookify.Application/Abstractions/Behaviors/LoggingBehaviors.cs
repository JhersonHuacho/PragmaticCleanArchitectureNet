using Bookify.Domain.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Bookify.Application.Abstractions.Behaviors
{
	public class LoggingBehaviors<TRequest, TResponse>
		: IPipelineBehavior<TRequest, TResponse>
		where TRequest : IBaseRequest
		where TResponse : Result
	{
		private readonly ILogger<LoggingBehaviors<TRequest, TResponse>> _logger;

		public LoggingBehaviors(ILogger<LoggingBehaviors<TRequest, TResponse>> logger)
		{
			_logger = logger;
		}

		public async Task<TResponse> Handle(
			TRequest request, 
			RequestHandlerDelegate<TResponse> next, 
			CancellationToken cancellationToken)
		{
			var name = request.GetType().Name;

			try
			{
				_logger.LogInformation("Executing request {Request} processing started", name);
				
				var result = await next();
				
				if (result.IsSuccess)
				{
					_logger.LogInformation("Request {Request} processed successfully", name);
				}
				else
				{
					//_logger.LogError("Request {Request} processed with {@Error}", name, result.Error);

					using (LogContext.PushProperty("Error", result.Error, true))
					{
						_logger.LogError("Request {Request} processed with error", name);
					}
				}
				

				return result;
			} 
			catch (Exception exception)
			{
				_logger.LogError(exception, "Request {Request} processing failed", name);
				throw;
			}
		}
	}
}
