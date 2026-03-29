using Microsoft.AspNetCore.Mvc;
using Moq;
using WageringFeed.API.Application.Interfaces;
using WageringFeed.API.Application.Models;
using WageringFeed.API.Web.Controllers;

namespace WageringFeed.API.Tests;

public class CustomerControllerTests
{
    private readonly Mock<ICustomerService> _mockService;
    private readonly CustomerController _controller;

    public CustomerControllerTests()
    {
        _mockService = new Mock<ICustomerService>();
        _controller = new CustomerController(_mockService.Object);
    }

    [Fact]
    public async Task GetCustomerStats_ReturnsNotFoundWhenServiceReturnsNull()
    {
        _mockService.Setup(s => s.GetCustomerStats(99)).ReturnsAsync((CustomerStatsResponse?)null);

        var result = await _controller.GetCustomerStats(99);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Customer Id not found", notFound.Value);
    }

    [Fact]
    public async Task GetCustomerStats_ReturnsOkWithBodyWhenFound()
    {
        var dto = new CustomerStatsResponse { CustomerId = 1, Name = "A", TotalStandToWin = 3m };
        _mockService.Setup(s => s.GetCustomerStats(1)).ReturnsAsync(dto);

        var result = await _controller.GetCustomerStats(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(dto, ok.Value);
    }

    [Fact]
    public async Task GetCustomers_ReturnsOkWithList()
    {
        var list = new List<CustomerDetails> { new() { CustomerId = 2, Name = "B" } };
        _mockService.Setup(s => s.GetCustomers()).ReturnsAsync(list);

        var result = await _controller.GetCustomers();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(list, ok.Value);
    }
}
