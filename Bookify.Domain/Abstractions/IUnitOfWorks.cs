using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Domain.Abstractions
{
	public interface IUnitOfWorks
	{
		Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}
