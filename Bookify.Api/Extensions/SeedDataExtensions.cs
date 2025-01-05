using Bogus;
using Bookify.Application.Abstractions.Data;
using Bookify.Domain.Apartments;
using Dapper;

namespace Bookify.Api.Extensions
{
	public static class SeedDataExtensions
	{
		public static void SeedData(this IApplicationBuilder app)
		{
			using var scope = app.ApplicationServices.CreateScope();

			var sqlConnectionFactory = scope.ServiceProvider.GetRequiredService<ISqlConnectionFactory>();

			using var connection = sqlConnectionFactory.CreateConnection();

			var faker = new Faker();

			List<object> apartments = new List<object>();

			for (int i = 0; i < 100; i++)
			{
				apartments.Add(new
				{
					Id = Guid.NewGuid(),
					Name = faker.Commerce.ProductName(),
					Description = faker.Lorem.Paragraph(),
					Country = faker.Address.Country(),
					State = faker.Address.State(),
					ZipCode = faker.Address.ZipCode(),
					City = faker.Address.City(),
					Street = faker.Address.StreetAddress(),
					PriceAmount = faker.Random.Decimal(50, 1000),
					PriceCurrency = "USD",
					CleaningFeeAmount = faker.Random.Decimal(25, 200),
					CleaningFeeCurrency = "USD",
					Amenities = new List<int> { (int)Amenity.Parking, (int)Amenity.MountainView },
					LastBookedOn = DateTime.MinValue
					//Address = new
					//{
					//	Street = faker.Address.StreetName(),
					//	City = faker.Address.City(),
					//	ZipCode = faker.Address.ZipCode(),
					//	Country = faker.Address.Country()
					//}
				});
			}

			// TODO: Add more seed data			
			//const string sql = "INSERT INTO public.apartments " +
			//	"(id, 'name', description, address_country, address_state, address_zip_code, address_city, address_street) " +
			//	"VALUES (@Id, @Name, @Description, @Country, @State, @ZipCode, @City, @Street, @PriceAmount, @PriceCurrency, @CleaningFeeAmount)";

			const string sql = "INSERT INTO public.apartments " +
				"(id, name, description, address_country, address_state, address_zip_code, address_city, address_street, price_amount, price_currency, cleaning_fee_amount, cleaning_fee_currency, amenities, last_booked_on_utc, version) " +
				"VALUES(@Id, @Name, @Description, @Country, @State, @ZipCode, @City, @Street, @PriceAmount, @PriceCurrency, @CleaningFeeAmount, @CleaningFeeCurrency, @Amenities, @LastBookedOn, 0);";

			connection.Execute(sql, apartments);
		}
	}
}
