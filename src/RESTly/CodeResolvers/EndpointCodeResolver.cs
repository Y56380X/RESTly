// Copyright Y56380X https://github.com/Y56380X/RESTly.
// Licensed under the MIT License.

using System.Text;
using Microsoft.OpenApi.Models;
using RequestDefinition = (string ContentType, Microsoft.OpenApi.Models.OpenApiMediaType RequestType);

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

		RequestDefinition? request = operation.RequestBody is { Content: var requestContent } && requestContent.Any()
			? new RequestDefinition(requestContent.FirstOrDefault().Key, requestContent.FirstOrDefault().Value)
			: null;
		var response = operation.Responses
			.SelectMany(r => r.Value.Content.Select(c => c.Value))
			.FirstOrDefault();
		
		var callsCode = GenerateCallCode(
			pathTemplate, 
			operationType, 
			operation.Parameters.ToArray(), 
			operation.OperationId, 
			request, 
			response);
		
		return callsCode;
	}

	private static string GenerateCallCode(string pathTemplate, OperationType operationType, 
		OpenApiParameter[] parameters, string? operationId,
		RequestDefinition? request, OpenApiMediaType? response)
	{
		var methodName = GenerateMethodName();
		var methodArguments = parameters
			.Select(GenerateMethodArgumentCode)
			.ToList();
		var responseType = response is { Schema: not null }
			? $"Response<{response.Schema.ToCsType(forceNullable: true)}>"
			: "Response";
		var httpMethod = HttpMethodMapping[operationType];
		var preparedPathTemplate = GeneratePreparedPathTemplate();

		var callCodeBuilder = new StringBuilder();
		callCodeBuilder.AppendLine($"""using var request = new HttpRequestMessage({httpMethod}, $"{preparedPathTemplate}");""");
		if (IsFormFileUpload(out var multipleFiles, out var formNames))
		{
			// todo: better handling of input of multiple file form fields!
			// => With the array upload-multiple can loose files!
			methodArguments.Insert(0, $"(Stream Stream, string FileName){(multipleFiles == true ? "[]" : string.Empty)} body");
			callCodeBuilder.AppendLine($"{"\t\t"}using var multipartContent = new MultipartFormDataContent();");
			switch (multipleFiles)
			{
				case true when formNames is { Length: > 0 }: // unroll file list from schema
					for (var fileIndex = 0; fileIndex < formNames.Length; fileIndex++)
						callCodeBuilder.AppendLine($"{"\t\t"}multipartContent.Add(new StreamContent(body[{fileIndex}].Stream), \"{formNames[fileIndex]}\", body[{fileIndex}].FileName);");
					break;
				case true:
					callCodeBuilder.AppendLine($"{"\t\t"}for (var fileIndex = 0; fileIndex < body.Length; fileIndex++)");
					callCodeBuilder.AppendLine($"{"\t\t\t"}multipartContent.Add(new StreamContent(body[fileIndex].Stream), $\"file{{fileIndex}}\", body[fileIndex].FileName);");
					break;
				default:
					callCodeBuilder.AppendLine($"{"\t\t"}multipartContent.Add(new StreamContent(body.Stream), \"file\", body.FileName);");
					break;
			}
			callCodeBuilder.AppendLine($"{"\t\t"}request.Content = multipartContent;");
		}
		else if (request is { RequestType.Schema: not null })
		{
			methodArguments.Insert(0, $"{request.Value.RequestType.Schema.ToCsType()} body");
			callCodeBuilder.AppendLine($"{"\t\t"}request.Content = JsonContent.Create(body, options: _jsonOptions);");
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
			{
				var operationMethodName = operationId.EndsWith("Async") ? operationId : $"{operationId}Async";
				return operationMethodName.NormalizeCsName();
			}
			
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
					? $"{{string.Join(\"&\", {p.Name}.Select(x => $\"{GenerateParameterAssignment(p, "x")}\"))}}"
					: GenerateParameterAssignment(p, p.Name))
				.ToArray();
			var baseUrl = pathTemplate.TrimStart(['/', '\\']);
			var path = queryParameters.Any() ? 
				$"{baseUrl}?{string.Join("&", queryParameters)}"
				: baseUrl;
			return path;

			string GenerateParameterAssignment(OpenApiParameter parameter, string memberName) =>
				parameter.Schema?.Type switch
				{
					"string" when !parameter.Schema.Enum.Any()
						=> $"{parameter.Name}={{HttpUtility.UrlEncode({memberName})}}",
					_   => $"{parameter.Name}={{{memberName}}}"
				};
		}

		bool IsFormFileUpload(out bool? multiple, out string[]? formNames)
		{
			if (request is { ContentType: { } ct } 
			    && ct.Equals("multipart/form-data", StringComparison.InvariantCultureIgnoreCase)
			    && request is { RequestType.Schema: { } s})
			{
				var isFileUpload = 
					(s.Format ?? s.Items?.Format)?.Equals("binary", StringComparison.InvariantCultureIgnoreCase) == true
					|| (s.Type == "object" && (s.Properties?.Any(p => p.Value.Format == "binary") ?? false)); // todo: use .Equals() ??
				formNames = s.Properties?
					.Where(p => p.Value.Format?.Equals("binary", StringComparison.InvariantCultureIgnoreCase) == true)
					.Select(p => p.Key)
					.ToArray() ?? [];
				multiple = 
					formNames.Length > 1
					|| s.Items?.Format?.Equals("binary", StringComparison.InvariantCultureIgnoreCase) == true;
				return isFileUpload;
			}

			formNames = null;
			multiple = null;
			return false;
		}
	}
}
