using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Abstractions.Data;
using Bookify.Application.Abstractions.Email;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Apartments;
using Bookify.Domain.Bookings;
using Bookify.Domain.Users;
using Bookify.Infrastructure.Clock;
using Bookify.Infrastructure.Data;
using Bookify.Infrastructure.Email;
using Bookify.Infrastructure.Repositories;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bookify.Infrastructure
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructure(
			this IServiceCollection services, 
			IConfiguration configuration)
		{
			services.AddTransient<IDateTimeProvider, DateTimeProvider>();
			services.AddTransient<IEmailService, EmailService>();

			var connectionString = configuration.GetConnectionString("Database") 
				?? throw new ArgumentNullException(nameof(configuration));

			#region Agregar DbContext a la inyección de dependencias y configurar la conexión a la base de datos
			services.AddDbContext<ApplicationDbContext>(options =>
			{
				options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
			});
			#endregion

			#region Agregar los repositorios a la inyección de dependencia
			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<IApartmentRepository, ApartmenRepository>();
			services.AddScoped<IBookingRepository, BookingRepository>();
			
			services.AddScoped<IUnitOfWorks>(sp => sp.GetRequiredService<ApplicationDbContext>());
			#endregion

			#region Estas líneas de código se encargan de configurar la conexión a la base de datos y de agregar un manejador de tipos para el tipo DateOnly
			services.AddSingleton<ISqlConnectionFactory>(_ => new SqlConnectionFactory(connectionString));
			SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
			#endregion

			return services;
		}
	}
}
