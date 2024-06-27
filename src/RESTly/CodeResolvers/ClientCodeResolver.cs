// Copyright Y56380X https://github.com/Y56380X/RESTly.
// Licensed under the MIT License.

using Microsoft.OpenApi.Models;

namespace Restly.CodeResolvers;

internal sealed class ClientCodeResolver : CodeResolverBase
{
	private readonly OpenApiDocument _apiSpecification;
	
	public ClientCodeResolver(OpenApiDocument apiSpecification)
	{
		_apiSpecification = apiSpecification;
	}

	protected override string Resolve()
	{
		// Generate request and response models code
		var modelsCode = _apiSpecification.Components.Schemas
			.Select(schema => new ComponentCodeResolver(schema.Key, schema.Value))
			.Select(mcr => mcr.GeneratedCode);

		// Generate REST call methods for API client
		var callsCode = _apiSpecification.Paths
			.Select(path => new EndpointCodeResolver(path.Key, path.Value))
			.Select(ecr => ecr.GeneratedCode)
			.Where(c => !string.IsNullOrWhiteSpace(c));
		
		var clientCode =
			$$"""
			  // <auto-generated/>
			  using System;
			  using System.Linq;
			  using System.Net.Http;
			  using System.Net.Http.Json;
			  using System.Text.Json;
			  using System.Text.Json.Serialization;
			  using System.Threading;
			  using System.Threading.Tasks;
			  using System.Web;

			  #nullable enable

			  namespace Restly;

			  public partial class {{_apiSpecification.Info.Title}} : IDisposable
			  {
			  {{"\t"}}private readonly HttpClient _httpClient;
			  {{"\t"}}private readonly JsonSerializerOptions _jsonOptions;

			  {{"\t"}}public {{_apiSpecification.Info.Title}}(HttpClient httpClient)
			  {{"\t"}}{
			  {{"\t\t"}}_httpClient  = httpClient;
			  {{"\t\t"}}_jsonOptions = new JsonSerializerOptions
			  {{"\t\t"}}{
			  {{"\t\t\t"}}PropertyNameCaseInsensitive = true
			  {{"\t\t"}}};
			  {{"\t"}}}

			  {{"\t"}}public {{_apiSpecification.Info.Title}}(HttpClient httpClient, JsonSerializerOptions jsonOptions)
			  {{"\t"}}{
			  {{"\t\t"}}_httpClient  = httpClient;
			  {{"\t\t"}}_jsonOptions = jsonOptions;
			  {{"\t"}}}
			  
			  {{string.Join("\n\n", callsCode)}}

			  {{"\t"}}public void Dispose()
			  {{"\t"}}{
			  {{"\t\t"}}_httpClient.Dispose();
			  {{"\t"}}}

			  {{string.Join("\n\n", modelsCode)}}
			  }
			  """;

		return clientCode;
	}
}