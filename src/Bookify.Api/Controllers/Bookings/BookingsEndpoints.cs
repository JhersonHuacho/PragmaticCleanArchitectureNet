using Asp.Versioning;
using Asp.Versioning.Builder;
using Bookify.Application.Bookings.GetBooking;
using Bookify.Application.Bookings.ReserveBooking;
using MediatR;

namespace Bookify.Api.Controllers.Bookings;

public static class BookingsEndpoints
{
	public static IEndpointRouteBuilder MapBookingsMinimalEndpoints(this IEndpointRouteBuilder builder)
	{
		ApiVersionSet apiVersionSet = builder.NewApiVersionSet()
			.HasApiVersion(new ApiVersion(1, 0))			
			.ReportApiVersions()
			.Build();

		builder.MapGet("api/v{version:apiVersion}/bookings-minimal/{id}", GetBooking)
			.RequireAuthorization()//.RequireAuthorization("bookings:read");
			.WithName(nameof(GetBooking))
			.WithApiVersionSet(apiVersionSet);

		builder.MapPost("api/v{version:apiVersion}/bookings-minimal", ReserveBooking)
			.RequireAuthorization();

		builder.MapPost("api/v{version:apiVersion}/bookings-minimal/dos", ReserveBookingV2)
			.RequireAuthorization()
			.WithApiVersionSet(apiVersionSet);

		return builder;
	}
	
	public static async Task<IResult> GetBooking(Guid id, ISender sender, CancellationToken cancellationToken) 
	{
		var query = new GetBookingQuery(id);

		var result = await sender.Send(query, cancellationToken);

		return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
	}
	
	public static async Task<IResult> ReserveBooking(
		ReserveBookingRequest request,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var command = new ReserveBookingCommand(
			request.ApartmentId,
			request.UserId,
			DateOnly.FromDateTime(request.StartDate),
			DateOnly.FromDateTime(request.EndDate));

		var result = await sender.Send(command, cancellationToken);

		if (result.IsFailure)
		{
			return Results.BadRequest(result.Error);
		}

		return Results.CreatedAtRoute(nameof(GetBooking), new { id = result.Value }, result.Value);
	}
	
	public static async Task<IResult> ReserveBookingV2(
		ReserveBookingDosRequest request,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var command = new ReserveBookingCommand(
			request.ApartmentId,
			request.UserId,
			request.StartDate,
			request.EndDate);

		var result = await sender.Send(command, cancellationToken);

		if (result.IsFailure)
		{
			return Results.BadRequest(result.Error);
		}

		return Results.CreatedAtRoute(nameof(GetBooking), new { id = result.Value }, result.Value);
	}
}
