# RESTly

A C# source generator for generating REST clients out of OpenApi specifications.
**Currently, supports .NET 8 and 9 SDK.** Generated code can target as low as netstandard2.0, but with additional polyfills needed.

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

## Target older framework versions

Building RESTly client libraries makes a new .NET SDK version necessary.
Target is to support at least the latest LTS version (but no guarantee).

With RESTly it is possible to build REST clients for older .NET versions 
netstandard 2.0 and onwards compatible (, see [https://learn.microsoft.com/en-us/dotnet/standard/net-standard](https://learn.microsoft.com/en-us/dotnet/standard/net-standard)).
When building for older .NET versions, additional dependencies might become necessary.
This can be seen in the `ClientLibrary` example project.

## File uploads

There is one possible issue with file upload.
When using minimal APIs with the `.WithOpenApi()` extension method and
call parameters of type `IFormFile` the generated schema contains no names
for the form field names. This makes the automated generation of client code hardly possible.
For using such endpoints the correct names must be given in the `Name` field of the body parameter.
