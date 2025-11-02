using Bookify.Application.Apartments.SearchApartments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Api.Controllers.Apartments;

[Authorize]
[ApiController]
[Route("api/apartments")]
public class ApartmentsController : ControllerBase
{
	private readonly ISender _sender;

    public ApartmentsController(ISender sender)
    {
        _sender = sender;
	}

	[HttpGet]
	public async Task<IActionResult> SearchsApartments(
		[FromQuery] DateTime startDate,
		[FromQuery] DateTime endDate,
		CancellationToken cancellationToken)
	{
		var query = new SearchApartmentsQuery(
			DateOnly.FromDateTime(startDate), 
			DateOnly.FromDateTime(endDate));

		var result = await _sender.Send(query, cancellationToken);

		return Ok(result.Value);
	}

	[HttpGet("dos")]
	public async Task<IActionResult> SearchsApartmentsV2(
		[FromQuery] DateOnly startDate,
		[FromQuery] DateOnly endDate,
		CancellationToken cancellationToken)
	{
		var query = new SearchApartmentsQuery(startDate, endDate);

		var result = await _sender.Send(query, cancellationToken);

		return Ok(result.Value);
	}
}
