using Bookify.Api.Middleware;
using Bookify.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Api.Extensions
{
	public static class ApplicationBuilderExtensions
	{
		// this method ApplyMigration is used to apply the migration to the database for local development purposes
		// to take the application builder and create a scope, use this scope to resolve my database context and the apply any pending migrations to my database
		public static void ApplyMigration(this IApplicationBuilder app) 
		{
			using var scope = app.ApplicationServices.CreateScope();
			
			using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

			dbContext.Database.Migrate();
		}

		public static void UseCustomExceptionHandler(this IApplicationBuilder app)
		{
			app.UseMiddleware<ExceptionHandlingMiddleware>();
		}
	}
}
