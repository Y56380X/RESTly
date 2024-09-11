﻿using System.Text.Json;
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
var uploadCollectionResult = await simpleApi.PostUploadCollectionAsync([
	(fileUpload3, "some_file_3.txt"),
	(fileUpload4, "some_file_4.txt"),
	(fileUpload5, "some_file_5.txt")]);
Console.WriteLine(uploadCollectionResult);

var floors = await simpleApi.GetFloorsAsync();
Console.WriteLine(floors.Model?.FirstOrDefault());

[JsonSerializable(typeof(SimpleApi.WeatherForecast[]))]
[JsonSerializable(typeof(SimpleApi.FloorItem[]))]
[JsonSerializable(typeof(IDictionary<string, int>))]
[JsonSerializable(typeof(byte[]))]
[JsonSerializable(typeof(string[]))]
internal partial class SourceGenerationContext : JsonSerializerContext;
