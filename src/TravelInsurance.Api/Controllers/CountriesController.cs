using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelInsurance.Application.Plans.Abstractions;
using TravelInsurance.Application.Plans.DTOs;

namespace TravelInsurance.Api.Controllers;

[ApiController]
[Route("countries")]
[Authorize]
public class CountriesController(ICountryService countryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CountryResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var countries = await countryService.GetAllAsync(cancellationToken);
        return Ok(countries);
    }
}
