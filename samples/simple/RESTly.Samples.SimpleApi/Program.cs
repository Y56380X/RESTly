using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
	"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
	{
		var forecast = Enumerable.Range(1, 5).Select(index =>
				new WeatherForecast
				(
					DateOnly.FromDateTime(DateTime.Now.AddDays(index)).ToDateTime(TimeOnly.MaxValue),
					Random.Shared.Next(-20, 55),
					summaries[Random.Shared.Next(summaries.Length)]
				))
			.ToArray();
		return forecast;
	})
	.WithName("GetWeatherForecast")
	.WithOpenApi();

app.MapPost("/file-upload", (byte[] content) =>
	{
		Console.WriteLine($"Uploaded {content.Length} bytes!");
	})
	.WithName("FileUpload")
	.WithOpenApi();

app.MapGet("/dictionary", () => new Dictionary<string, int>
	{
		{"Monday", 30},
		{"Friday", 10}
	})
	.WithName("GetDictionary")
	.WithOpenApi();

app.MapGet("/cities", ([FromQuery(Name = "q")]string? query) =>
	{
		string[] cities =
		[
			"Rome",
			"Milan",
			"Berlin",
			"Barcelona",
			"Turin",
			"Praha"
		];
		return !string.IsNullOrWhiteSpace(query)
			? cities.Where(c => c.StartsWith(query, StringComparison.InvariantCultureIgnoreCase))
			: cities ;
	})
	.WithName("QueryCities")
	.WithOpenApi();

app.MapGet("/things", ([FromQuery(Name = "q")]int[] query) =>
	{
		string[] things =
		[
			"Apple",
			"Shelf",
			"Car",
			"Train",
			"Table",
			"Door"
		];
		return query.Select(i => things[i]);
	})
	.WithName("QueryThingsByIndex")
	.WithOpenApi();

var items = new[]
{
	new Item(Guid.NewGuid(), "Item 1", ItemType.TestItem1),
	new Item(Guid.NewGuid(), "Item 2", ItemType.TestItem2)
};

app.MapGet("/items", () => items)
	.WithOpenApi();

app.MapGet("/items/{type}", (ItemType type) => items.Where(i => i.Type == type))
	.WithOpenApi();

app.Run();

record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
	public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record Item(Guid Uuid, string Name, ItemType Type);

[JsonConverter(typeof(JsonStringEnumConverter))]
enum ItemType
{
	TestItem1,
	TestItem2
}
