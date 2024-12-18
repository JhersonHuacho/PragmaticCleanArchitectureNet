using Bookify.Application.Apartments.SearchApartments;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Api.Controllers.Apartments
{
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
			[FromQuery] DateOnly startDate,
			[FromQuery] DateOnly endDate,
			CancellationToken cancellationToken)
		{
			var query = new SearchApartmentsQuery(startDate, endDate);

			var result = await _sender.Send(query, cancellationToken);

			return Ok(result.Value);
		}
	}
}
