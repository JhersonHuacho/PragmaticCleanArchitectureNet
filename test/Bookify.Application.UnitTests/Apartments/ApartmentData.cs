using Bookify.Domain.Apartments;
using Bookify.Domain.Shared;

namespace Bookify.Application.UnitTests.Apartments
{
	internal static class ApartmentData
	{
		public static Apartment Create()
		{
			return new Apartment(
				Guid.NewGuid(),
				new Name("Test Apartment"),
				new Description("Test Description."),
				new Address("Country", "State", "ZipCode", "City", "Street"),
				new Money(100.0m, Currency.Usd),
				Money.Zero(),
				new List<Amenity>()
			);
		}
	}
}
