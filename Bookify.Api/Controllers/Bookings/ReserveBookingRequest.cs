namespace Bookify.Api.Controllers.Bookings
{
	public sealed record ReserveBookingRequest(
		Guid ApartmentId,
		Guid UserId,
		//DateOnly StartDate,
		DateTime StartDate,
		//DateOnly EndDate);
		DateTime EndDate);

	public sealed record ReserveBookingDosRequest(
		Guid ApartmentId,
		Guid UserId,
		DateOnly StartDate,
		DateOnly EndDate);
}
