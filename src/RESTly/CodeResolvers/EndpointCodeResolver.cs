// Copyright Y56380X https://github.com/Y56380X/RESTly.
// Licensed under the MIT License.

using System.Text;
using Microsoft.OpenApi.Models;

namespace Restly.CodeResolvers;

internal class EndpointCodeResolver : CodeResolverBase
{
	private static readonly IDictionary<OperationType, string> HttpMethodMapping =
		new Dictionary<OperationType, string>
		{
			{ OperationType.Head  , "HttpMethod.Head"   },
			{ OperationType.Get   , "HttpMethod.Get"    },
			{ OperationType.Post  , "HttpMethod.Post"   },
			{ OperationType.Put   , "HttpMethod.Put"    },
			{ OperationType.Delete, "HttpMethod.Delete" }
		};
	
	private readonly OpenApiPathItem _pathItem;
	private readonly string _pathTemplate;

	public EndpointCodeResolver(string pathTemplate, OpenApiPathItem pathItem)
	{
		_pathTemplate = pathTemplate;
		_pathItem = pathItem;
	}
	
	protected override string Resolve()
	{
		var callsCode = _pathItem.Operations
			.Select(kvp => GenerateOperationCode(_pathTemplate, kvp.Key, kvp.Value))
			.Where(c => c is not null); // filter not generated code for not supported operations
		return string.Join("\n\n", callsCode);
	}
	
	private static string? GenerateOperationCode(string pathTemplate, OperationType operationType, OpenApiOperation operation)
	{
		// Check for operation type support
		if (!HttpMethodMapping.ContainsKey(operationType))
			return null;

		var request = operation.RequestBody is { Content: var requestContent } && requestContent.Any()
			? requestContent.Values.FirstOrDefault()
			: null;
		var response = operation.Responses
			.SelectMany(r => r.Value.Content.Select(c => c.Value))
			.FirstOrDefault();
		
		var callsCode = GenerateCallCode(
			pathTemplate, operationType, operation.Parameters.ToArray(), operation.OperationId, request, response);
		return callsCode;
	}

	private static string GenerateCallCode(string pathTemplate, OperationType operationType, 
		OpenApiParameter[] parameters, string? operationId,
		OpenApiMediaType? request, OpenApiMediaType? response)
	{
		var methodName = GenerateMethodName();
		var methodArguments = parameters
			.Select(GenerateMethodArgumentCode)
			.ToList();
		var responseType = response is { Schema: not null}
			? $"Response<{response.Schema.ToCsType(forceNullable: true)}>"
			: "Response";
		var httpMethod = HttpMethodMapping[operationType];
		var preparedPathTemplate = GeneratePreparedPathTemplate();

		var callCodeBuilder = new StringBuilder();
		callCodeBuilder.AppendLine($"""using var request = new HttpRequestMessage({httpMethod}, $"{preparedPathTemplate}");""");
		if (request is { Schema: not null })
		{
			methodArguments.Insert(0, $"{request.Schema.ToCsType()} body");
			callCodeBuilder.AppendLine($"{"\t\t"}request.Content = JsonContent.Create(body);");
		}
		callCodeBuilder.AppendLine($"{"\t\t"}using var response = await _httpClient.SendAsync(request, cancellationToken);");
		if (response is { Schema: not null })
		{
			callCodeBuilder.AppendLine($"{"\t\t"}{response.Schema.ToCsType(forceNullable: true)} model;");
			callCodeBuilder.AppendLine($"{"\t\t"}if (response.IsSuccessStatusCode)");
			callCodeBuilder.AppendLine($"{"\t\t\t"}model = JsonSerializer.Deserialize<{response.Schema.ToCsType()}>(await response.Content.ReadAsStreamAsync(cancellationToken), _jsonOptions);");
			callCodeBuilder.AppendLine($"{"\t\t"}else");
			callCodeBuilder.AppendLine($"{"\t\t\t"}model = default;");
		}
		
		var responseArguments = new List<string>
		{
			"response.IsSuccessStatusCode",
			"response.StatusCode"
		};
		if (response != null) responseArguments.Add("model");
		
		var callCode = 
			$$"""
			  {{"\t"}}public async Task<{{responseType}}> {{methodName}}({{string.Join(", ", methodArguments.Append("CancellationToken cancellationToken = default"))}})
			  {{"\t"}}{
			  {{"\t\t"}}{{callCodeBuilder}}
			  {{"\t\t"}}return new {{responseType}}({{string.Join(", ", responseArguments)}});
			  {{"\t"}}}
			  """;
		
		return callCode;
		
		string GenerateMethodName()
		{
			// use `operationId` as first priority
			if (operationId != null && !string.IsNullOrWhiteSpace(operationId))
				return operationId.EndsWith("Async") ? operationId : $"{operationId}Async";
			
			// build method name based on path template
			var methodFragments = pathTemplate
				.Split([' ', '/', '\\', '-', '_', '.', ':', '{', '}', '(', ')', '[', ']'], StringSplitOptions.RemoveEmptyEntries)
				.Select(f => f.Capitalize());
			
			return $"{operationType}{string.Concat(methodFragments)}Async"; // todo: generate better methods names (take response and parameters into account)
		}

		string GenerateMethodArgumentCode(OpenApiParameter parameter) => 
			$"{parameter.Schema.ToCsType()} {parameter.Name}";

		string GeneratePreparedPathTemplate()
		{
			var queryParameters = parameters
				.Where(p => p.In == ParameterLocation.Query)
				.Select(p => p.Schema?.Type == "array"
					? $"{{string.Join(\"&\", {p.Name}.Select(x => $\"{p.Name}={{HttpUtility.UrlEncode(x)}}\"))}}"
					: $"{p.Name}={{HttpUtility.UrlEncode({p.Name})}}")
				.ToArray();
			var baseUrl = pathTemplate.TrimStart(['/', '\\']);
			var path = queryParameters.Any() ? 
				$"{baseUrl}?{string.Join("&", queryParameters)}"
				: baseUrl;
			return path;
		}
	}
}
