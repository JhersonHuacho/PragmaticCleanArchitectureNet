using Bookify.Domain.Abstractions;

namespace Bookify.Domain.UnitTests.Infrastructure
{
	public abstract class BaseTest
	{
		public static T AssertDomainEventWasPublished<T>(Entity entity)
			where T : IDomainEvent
		{
			var domainEvent = entity.GetDomainEvents().OfType<T>().FirstOrDefault();
			
			if (domainEvent == null)
			{
				throw new Exception($"Expected domain event of type {typeof(T).Name} was not published.");
			}

			return domainEvent;
		}
	}
}
