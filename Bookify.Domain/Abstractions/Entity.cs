namespace Bookify.Domain.Abstractions
{
	public abstract class Entity
	{
		private readonly List<IDomainEvent> _domainEvents = new();

		protected Entity(Guid id)
        {
			Id = id;
		}

		protected Entity()
		{

		}

		public Guid Id { get; init; }

		public IReadOnlyList<IDomainEvent> GetDomainEvents()
		{
			return _domainEvents.ToList();
		}

		public void ClearDomainEvents()
		{
			_domainEvents.Clear();
		}

		// mi metodo es protected quiere decir que solo las clases que heredan de Entity pueden llamar a este metodo
		protected void RaiseDomainEvent(IDomainEvent domainEvent)
		{
			_domainEvents.Add(domainEvent);
		}
	}
}
