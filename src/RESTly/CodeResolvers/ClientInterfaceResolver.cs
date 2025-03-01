// Copyright Y56380X https://github.com/Y56380X/RESTly.
// Licensed under the MIT License.

using Microsoft.OpenApi.Models;

namespace Restly.CodeResolvers;

internal sealed class ClientInterfaceResolver : CodeResolverBase
{
	private readonly OpenApiDocument _apiSpecification;
	private readonly List<string> _generatedMethodDefinitions;

	public ClientInterfaceResolver(OpenApiDocument apiSpecification, List<string> generatedMethodDefinitions)
	{
		_apiSpecification = apiSpecification;
		_generatedMethodDefinitions = generatedMethodDefinitions;
	}
	
	protected override string Resolve()
	{
		var clientClassName = _apiSpecification.Info.Title.Split('.').Last().NormalizeCsName();

		// Generate request and response models code
		var modelsCode = _apiSpecification.Components.Schemas
			.Select(schema => new ComponentCodeResolver(_apiSpecification, schema.Key, schema.Value))
			.Select(mcr => mcr.GeneratedCode);
		
		var clientCode =
			$$"""
			  // <auto-generated/>
			  // Copyright Y56380X https://github.com/Y56380X/RESTly.
			  // Licensed under the MIT License.
			  
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

			  public interface I{{clientClassName}}
			  {
			  {{'\t'}}{{string.Join("\n\n\t", _generatedMethodDefinitions.Select(m => $"{m.Replace(" async ", " ")};"))}}

			  {{string.Join("\n\n", modelsCode)}}
			  }
			  """;

		return clientCode;
	}
}