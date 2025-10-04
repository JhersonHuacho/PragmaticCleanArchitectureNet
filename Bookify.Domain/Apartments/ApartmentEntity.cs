namespace Bookify.Domain.Apartments
{
	public sealed class ApartmentEntity
	{
		/* Lo que tenemos aquí es una entidad Apartment que es lo que llamaría a un modelo de dominio anémico,
		 * lo que significa que no tiene lógica de negocio, solo tiene propiedades,
		 * que solo actúa como un contenedor de datos con un montón de propiedades públicas con getters y setters,
		 * y no tiene ningún comportamiento o lógica asociada a ella.
		 */
		/*
		 * Country,State,ZipCode,City y Street son propiedades que son string.
		 * Estos son llamados "primitive obsession" (obsesión por los tipos primitivos) en el diseño de software.
		 * Para solucionarlo podríamos crear una clase Address que encapsule estas propiedades.
		 * Esto se llama "introducir un objeto de valor" (introduce a value object) que represente una dirección.
		 * Con esto, podemos mejorar la cohesión y la claridad del código.
		 * Y también podemos tener nuestro modelo de dominio más rico y expresivo.
		 */
		public Guid Id { get; private set; }
		public string Name { get; private set; }
		public string Description { get; private set; }
		public string Country { get; private set; }
		public string State { get; private set; }
		public string ZipCode { get; private set; }
		public string City { get; private set; }
		public string Street { get; private set; }
		public decimal PriceAmount { get; private set; }
		public string PriceCurrency { get; private set; }
		public decimal CleaningAmount { get; private set; }
		public string CleaningFeeCurrency { get; private set; }
		public DateTime? LastBookedOnUtc { get; internal set; }
		public List<Amenity> Amenities { get; private set; } = new List<Amenity>();
	}
}
