using Bookify.Domain.Abstractions;

namespace Bookify.Domain.Users.Events
{
	public sealed record UserCreatedDomainEvents(Guid UserId) : IDomainEvent;
}
