﻿using Restly;

[assembly:RestlyClient("simple-api.yaml", "SimpleApi")]

using var httpClient = new HttpClient();
httpClient.BaseAddress = new Uri("http://localhost:5048");
using var simpleApi = new SimpleApi(httpClient);

var dictionaryResult = await simpleApi.GetDictionaryAsync();
Console.WriteLine(dictionaryResult);
Console.WriteLine(dictionaryResult.Model?.FirstOrDefault());

var fileUploadResult = await simpleApi.PostFileUploadAsync([12, 12, 12]);
Console.WriteLine(fileUploadResult);

var weatherResult = await simpleApi.GetWeatherforecastAsync();
Console.WriteLine(weatherResult);
Console.WriteLine(weatherResult.Model?.FirstOrDefault());
