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

using var fileUpload0 = new MemoryStream([100, 100, 200]);
var uploadSingleResult = await simpleApi.PostUploadSingleAsync((fileUpload0, "some_file.txt"));
Console.WriteLine(uploadSingleResult);

using var fileUpload1 = new MemoryStream([100, 100, 200]);
using var fileUpload2 = new MemoryStream([100, 100, 200, 100, 100, 200]);
var uploadMultipleResult = await simpleApi.PostUploadMultipleAsync([
	(fileUpload1, "some_file_1.txt"),
	(fileUpload2, "some_file_2.txt")]);
Console.WriteLine(uploadMultipleResult);

using var fileUpload3 = new MemoryStream([100, 100, 200]);
using var fileUpload4 = new MemoryStream([100, 100, 200, 100, 100, 200]);
using var fileUpload5 = new MemoryStream([100, 100, 200, 100, 100, 200, 100, 100, 200]);
var uploadCollectionResult = await simpleApi.PostUploadCollectionAsync(new SimpleApi.PostUploadCollectionBody([
	Convert.ToBase64String(fileUpload3.ToArray()),
	Convert.ToBase64String(fileUpload4.ToArray()),
	Convert.ToBase64String(fileUpload5.ToArray())]));
Console.WriteLine(uploadCollectionResult);

var floors = await simpleApi.GetFloorsAsync();
Console.WriteLine(floors.Model?.FirstOrDefault());

var polyItems1 = await simpleApi.GetDerivedTypesAsync();
foreach (var polyItem in polyItems1.Model ?? [])
	Console.WriteLine(polyItem);
await simpleApi.PostDerivedTypesAsync(new SimpleApi.FinalType1("Name1", 100));
await simpleApi.PostDerivedTypesAsync(new SimpleApi.FinalType2("Name1", 0.1d, 200d));
var polyItems2 = await simpleApi.GetDerivedTypesAsync();
foreach (var polyItem in polyItems2.Model ?? [])
	Console.WriteLine(polyItem);

[JsonSerializable(typeof(SimpleApi.WeatherForecast[]))]
[JsonSerializable(typeof(SimpleApi.SomeBaseItem[]))]
[JsonSerializable(typeof(SimpleApi.SomeTypeBase[]))]
[JsonSerializable(typeof(SimpleApi.PostUploadCollectionBody[]))]
[JsonSerializable(typeof(IDictionary<string, int>))]
[JsonSerializable(typeof(byte[]))]
[JsonSerializable(typeof(string[]))]
internal partial class SourceGenerationContext : JsonSerializerContext;
