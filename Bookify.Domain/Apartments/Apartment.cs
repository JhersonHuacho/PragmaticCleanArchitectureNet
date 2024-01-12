using Bookify.Domain.Abstractions;
using Bookify.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Domain.Apartments
{
    public sealed class Apartment : Entity
    {
        //public Apartment(Guid id) : base(id)
        //{
        //}
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
        public Name Name { get; private set; }
        public Description Description { get; private set; }
        public Address Address { get; private set; }
        //public decimal PriceAmount { get; private set; }
        //public string PriceCurrency { get; private set; }
        public Money Price { get; private set; }
        //public decimal CleaningFeeAmount { get; private set; }
        //public string CleaningFeeCurrency { get; private set; }
        public Money CleaningFee { get; private set; }
        public DateTime? LastBookendOnUtc { get; internal set; }
        public List<Amenity> Amenities { get; private set; } = new();
    }
}
