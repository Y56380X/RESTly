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
