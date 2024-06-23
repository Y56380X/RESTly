using System.Text.Json;
using System.Text.Json.Serialization;
using Restly;

using var httpClient = new HttpClient();
httpClient.BaseAddress = new Uri("http://localhost:5048");
using var simpleApi = new SimpleApi(httpClient, new JsonSerializerOptions
{
	PropertyNameCaseInsensitive = true,
	TypeInfoResolver = SourceGenerationContext.Default
});

var dictionaryResult = await simpleApi.GetDictionaryAsync();
Console.WriteLine(dictionaryResult);
Console.WriteLine(dictionaryResult.Model?.FirstOrDefault());

var fileUploadResult = await simpleApi.FileUploadAsync([12, 12, 12]);
Console.WriteLine(fileUploadResult);

var weatherResult = await simpleApi.GetWeatherForecastAsync();
Console.WriteLine(weatherResult);
Console.WriteLine(weatherResult.Model?.FirstOrDefault());

var citiesResult = await simpleApi.QueryCitiesAsync("b");
Console.WriteLine(citiesResult);
Console.WriteLine($"Cities: {string.Join(", ", citiesResult.Model ?? ArraySegment<string?>.Empty)}");

var thingsResult = await simpleApi.QueryThingsByIndexAsync([1, 4]);
Console.WriteLine(thingsResult);
Console.WriteLine($"Things: {string.Join(", ", thingsResult.Model ?? ArraySegment<string?>.Empty)}");

[JsonSerializable(typeof(SimpleApi.WeatherForecast[]))]
[JsonSerializable(typeof(IDictionary<string, int>))]
[JsonSerializable(typeof(byte[]))]
[JsonSerializable(typeof(string[]))]
internal partial class SourceGenerationContext : JsonSerializerContext;
