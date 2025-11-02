using Bookify.Domain.Abstractions;
using Bookify.Domain.Shared;

namespace Bookify.Domain.Apartments
{
	public sealed class Apartment : Entity
	{
        public Apartment(
            Guid id,
            Name name,
            Description description,
            Address address,
            Money price,
            Money cleaningFee,
            List<Amenity> amenities) 
            : base(id)
		{
			Name = name;
			Description = description;
			Address = address;
			Price = price;
			CleaningFee = cleaningFee;
			Amenities = amenities;
		}

        private Apartment()
        {

        }

        public Name Name { get; private set; }
		public Description Description { get; private set; }
        public Address Address { get; private set; }
        //public decimal PriceAmount { get; private set; }
        //public string PriceCurrency { get; private set; }
        public Money Price { get; private set; }
        //public decimal CleaningAmount { get; private set; }
        //public string CleaningFeeCurrency { get; private set; }
        public Money CleaningFee { get; private set; }
        public DateTime? LastBookedOnUtc { get; internal set; }
		public List<Amenity> Amenities { get; private set; } = new List<Amenity>();
	}
}
