# RESTly

A (experimental, WIP) C# source generator for generating REST clients out of OpenApi specifications.
**Currently, supports .NET 8 SDK.** Generated code can target as low as netstandard2.0, but with additional polyfills needed.

## Getting Started

* Add NuGet package `RESTly` to your project
* Add a OpenApi specification as `AdditionalFiles` to your project
* Add assembly attribute `RestlyClient` into your code with the name of the OpenApi specification file and the name of the api client to generate
* The generated API client is located in the `Restly` namespace

## Native AOT

This source generator generates AOT compatible code from version 0.7 onwards.
For using AOT there is the need to add a client library project additionally to the application itself.
In the application project the JSON serialization context for the generated API models must be defined.
The split into projects is necessary as RESTly and JSON serialization context need source generators and
those can not be chained.
