using Bookify.Domain.Apartments;

namespace Bookify.Infrastructure.Repositories
{
	internal sealed class ApartmenRepository : Repository<Apartment>, IApartmentRepository
	{
		public ApartmenRepository(ApplicationDbContext dbContext) : base(dbContext)
		{
		}
	}	
}
