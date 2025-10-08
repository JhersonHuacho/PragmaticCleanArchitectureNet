using Bookify.Application.Abstractions.Behaviors;
using Bookify.Domain.Bookings;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Bookify.Application;

public static class DependencyInjection
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		services.AddMediatR(configuration => 
		{
			configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
			configuration.AddOpenBehavior(typeof(LoggingBehaviors<,>));
			#region Add open behavior for validation
			configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
			#endregion
			configuration.AddOpenBehavior(typeof(QueryCachingBehavior<,>));
		});

		#region Add validators from assembly for validation. Using reflection to get all validators from the assembly.
		services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
		#endregion

		services.AddTransient<PricingService>();

		return services;
	}
}
