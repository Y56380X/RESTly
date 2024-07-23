using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAntiforgery();

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

app.UseAntiforgery();
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

// Sample for an endpoint with `IFormFile` in combination with `.WithOpenApi()`
// => the name of the parameter has to be given in the client.
app.MapPost("/upload-single", (IFormFile file) =>
	{
		Console.WriteLine($"Name = {file.Name}, FileName = {file.FileName}, Type = {file.ContentType}, Length = {file.Length}");
	})
	.DisableAntiforgery();

app.MapPost("/upload-multiple", (IFormFile file1, IFormFile file2) =>
	{
		Console.WriteLine($"Name = {file1.Name}, FileName = {file1.FileName}, Type = {file1.ContentType}, Length = {file1.Length}");
		Console.WriteLine($"Name = {file2.Name}, FileName = {file2.FileName}, Type = {file2.ContentType}, Length = {file2.Length}");
	})
	.DisableAntiforgery();

app.MapPost("/upload-collection", (IFormFileCollection files) =>
	{
		foreach (var file in files)
			Console.WriteLine($"Name = {file.Name}, FileName = {file.FileName}, Type = {file.ContentType}, Length = {file.Length}");
	})
	.DisableAntiforgery()
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
