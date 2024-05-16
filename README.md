# RESTly

A (experimental, WIP) C# source generator for generating REST clients out of OpenApi specifications.
**Currently supports .NET 8 SDK.**

## Getting Started

* Add NuGet package `RESTly` to your project
* Add a OpenApi specification as `AdditionalFiles` to your project
* Add assembly attribute `RestlyClient` into your code with the name of the OpenApi specification file and the name of the api client to generate
* The generated API client is located in the `Restly` namespace
