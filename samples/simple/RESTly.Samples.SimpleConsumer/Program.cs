using Restly;

[assembly:RestlyClient("simple-api.yaml", "SimpleApi")]

using var httpClient = new HttpClient();
httpClient.BaseAddress = new Uri("http://localhost:5048");
using var simpleApi = new SimpleApi(httpClient);

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
