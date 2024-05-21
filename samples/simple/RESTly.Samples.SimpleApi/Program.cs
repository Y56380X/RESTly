using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

app.MapGet("/cities", ([FromQuery(Name = "q")] string? query) =>
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

app.Run();

record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
	public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}