using System.Text.Json;
using System.Text.Json.Serialization;
using Scalar.AspNetCore;
using WageringFeed.API.Application.Interfaces;
using WageringFeed.API.Application.Configuration;
using WageringFeed.API.Application.Services;
using WageringFeed.API.Infrastructure.CustomerApi;
using WageringFeed.API.Infrastructure.Feed;
using WageringFeed.API.Infrastructure.Feed.Handlers;
using WageringFeed.API.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<WageringFeedOptions>(
    builder.Configuration.GetSection(WageringFeedOptions.SectionName));

builder.Services.AddSingleton(_ =>
{
    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    options.Converters.Add(new JsonStringEnumConverter());
    return options;
});

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddSingleton<ICustomerStore, CustomerStore>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

builder.Services.AddHttpClient<ICustomerApiClient, CustomerApiClient>();

builder.Services.AddSingleton<WageringFeedMessageDispatcher>();
builder.Services.AddSingleton<IWageringFeedMessageHandler, BetPlacedFeedMessageHandler>();
builder.Services.AddSingleton<IWageringFeedMessageHandler, FixtureFeedMessageHandler>();
builder.Services.AddSingleton<IWageringFeedMessageHandler, EndOfFeedMessageHandler>();

builder.Services.AddHostedService<WageringFeedConsumer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
