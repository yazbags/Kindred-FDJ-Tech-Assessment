using Microsoft.AspNetCore.Mvc;
using WageringFeed.API.Application.Interfaces;
using WageringFeed.API.Application.Models;

namespace WageringFeed.API.Web.Controllers;

[ApiController]
[Route("customer")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet("{customerId}/stats")]
    [ProducesResponseType(typeof(CustomerStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerStatsResponse>> GetCustomerStats(int customerId)
    {
        var result = await _customerService.GetCustomerStats(customerId);
        if (result is null)
        {
            return NotFound("Customer Id not found");
        }

        return Ok(result);
    }

    [HttpGet]
    // To help with testing so we can access available customerIds
    public async Task<IActionResult> GetCustomers()
    {
        var result = await _customerService.GetCustomers();
        return Ok(result);
    }
}
