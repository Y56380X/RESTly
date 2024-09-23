// Copyright Y56380X https://github.com/Y56380X/RESTly.
// Licensed under the MIT License.

using System.Text;
using System.Text.RegularExpressions;
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

	private readonly OpenApiComponents _components;
	private readonly OpenApiPathItem _pathItem;
	private readonly string _pathTemplate;

	public EndpointCodeResolver(string pathTemplate, OpenApiPathItem pathItem, OpenApiComponents components)
	{
		_pathTemplate = pathTemplate;
		_pathItem = pathItem;
		_components = components;
	}
	
	protected override string Resolve()
	{
		var callsCode = _pathItem.Operations
			.Select(kvp => GenerateOperationCode(_pathTemplate, kvp.Key, kvp.Value))
			.Where(c => c is not null); // filter not generated code for not supported operations
		return string.Join("\n\n", callsCode);
	}
	
	private string? GenerateOperationCode(string pathTemplate, OperationType operationType, OpenApiOperation operation)
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
		
		var callsCode = GenerateCallCode(pathTemplate, operationType, operation, request, response);
		
		return callsCode;
	}

	private string GenerateCallCode(string pathTemplate, OperationType operationType, 
		OpenApiOperation operation, RequestDefinition? request, OpenApiMediaType? response)
	{
		var parameters = operation.Parameters.ToArray();
		var operationId = operation.OperationId;
		
		var methodName = GenerateMethodName();
		var methodArguments = parameters
			.Select(GenerateMethodArgumentCode)
			.ToList();
		string responseType;
		string? generateResponseModelType;
		if (response is { Schema: not null })
		{
			var responseModelName = $"{methodName.Substring(0, methodName.Length - "Async".Length)}Result";
			var modelType = response.Schema.ToCsType(out var generate, responseModelName);
			responseType = $"Response<{modelType}>";
			generateResponseModelType = generate ? responseModelName : null;
		}
		else
		{
			generateResponseModelType = null;
			responseType = "Response";
		}
		var httpMethod = HttpMethodMapping[operationType];
		var preparedPathTemplate = GeneratePreparedPathTemplate();

		var callCodeBuilder = new StringBuilder();
		callCodeBuilder.AppendLine($"""using var request = new HttpRequestMessage({httpMethod}, $"{preparedPathTemplate}");""");

		string? generateRequestModelType;
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
			generateRequestModelType = null;
		}
		else if (request is { RequestType.Schema: not null })
		{
			var bodyTypeName = $"{methodName.Substring(0, methodName.Length - "Async".Length)}Body";
			methodArguments.Insert(0, $"{request.Value.RequestType.Schema.ToCsType(out var generate, bodyTypeName)} body");
			callCodeBuilder.AppendLine($"{"\t\t"}request.Content = JsonContent.Create(body, options: _jsonOptions);");
			generateRequestModelType = generate ? bodyTypeName : null;
		}
		else
		{
			generateRequestModelType = null;
		}
		
		callCodeBuilder.AppendLine($"{"\t\t"}using var response = await _httpClient.SendAsync(request, cancellationToken);");
		if (response is { Schema: not null })
		{
			callCodeBuilder.AppendLine($"{"\t\t"}{response.Schema.ToCsType(out _, generateResponseModelType, forceNullable: true)} model;");
			callCodeBuilder.AppendLine($"{"\t\t"}if (response.IsSuccessStatusCode)");
			callCodeBuilder.AppendLine($"{"\t\t\t"}model = JsonSerializer.Deserialize<{response.Schema.ToCsType(out _, generateResponseModelType)}>(await response.Content.ReadAsStreamAsync(cancellationToken), _jsonOptions);");
			callCodeBuilder.AppendLine($"{"\t\t"}else");
			callCodeBuilder.AppendLine($"{"\t\t\t"}model = default;");
		}
		
		var responseArguments = new List<string>
		{
			"response.IsSuccessStatusCode",
			"response.StatusCode"
		};
		if (response is { Schema: not null }) responseArguments.Add("model");
		
		var methodCode = 
			$$"""
			  {{"\t"}}public async Task<{{responseType}}> {{methodName}}({{string.Join(", ", methodArguments.Append("CancellationToken cancellationToken = default"))}})
			  {{"\t"}}{
			  {{"\t\t"}}{{callCodeBuilder}}
			  {{"\t\t"}}return new {{responseType}}({{string.Join(", ", responseArguments)}});
			  {{"\t"}}}
			  """;
		
		var finalCodeBuilder = new StringBuilder();
		if (!string.IsNullOrWhiteSpace(operation.Summary))
		{
			var operationSummary = operation.Summary.EndsWith(".") ? operation.Summary : $"{operation.Summary}.";
			finalCodeBuilder.AppendLine($"{'\t'}/// <summary>");
			foreach (var summaryPart in operationSummary.Split(['\n'], StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()))
				finalCodeBuilder.AppendLine($"{'\t'}///\t{summaryPart}");
			finalCodeBuilder.AppendLine($"{'\t'}/// </summary>");
		}
		
		finalCodeBuilder.Append(methodCode);
		
		// Write back generated types
		if (generateResponseModelType != null && response?.Schema is {} responseSchema)
			_components.Schemas.Add(generateResponseModelType, responseSchema);
		if (generateRequestModelType != null && request?.RequestType.Schema is {} requestSchema)
			_components.Schemas.Add(generateRequestModelType, requestSchema);
		
		return finalCodeBuilder.ToString();
		
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
			$"{parameter.Schema.ToCsType()} {parameter.Name.NormalizeCsName(false)}";

		string GeneratePreparedPathTemplate()
		{
			var queryParameters = parameters
				.Where(p => p.In == ParameterLocation.Query)
				.Select(p => p.Schema?.Type == "array"
					? $"{{string.Join(\"&\", {p.Name.NormalizeCsName(false)}.Select(x => $\"{GenerateParameterAssignment(p, "x")}\"))}}"
					: GenerateParameterAssignment(p, p.Name.NormalizeCsName(false)))
				.ToArray();
			
			// Generate `baseUrl`; path parameters should be normalized to C# names
			var pathParameterRegex = new Regex(@"\{([^\}]+)\}");
			var baseUrl = pathParameterRegex.Replace(pathTemplate, m => m.Result(m.Value.NormalizeCsName(false))).TrimStart('/', '\\');
			
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
