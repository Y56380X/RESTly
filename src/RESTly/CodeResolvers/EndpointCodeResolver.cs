// Copyright Y56380X https://github.com/Y56380X/RESTly.
// Licensed under the MIT License.

using System.Text;
using System.Text.RegularExpressions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using RequestDefinition = (string ContentType, Microsoft.OpenApi.Models.OpenApiMediaType RequestType);

namespace Restly.CodeResolvers;

internal class EndpointCodeResolver : CodeResolverBase
{
	private static readonly IDictionary<HttpMethod, string> HttpMethodMapping =
		new Dictionary<HttpMethod, string>
		{
			{ HttpMethod.Head  , "HttpMethod.Head"   },
			{ HttpMethod.Get   , "HttpMethod.Get"    },
			{ HttpMethod.Post  , "HttpMethod.Post"   },
			{ HttpMethod.Put   , "HttpMethod.Put"    },
			{ HttpMethod.Delete, "HttpMethod.Delete" }
		};

	private readonly OpenApiComponents _components;
	private readonly List<string> _generatedMethodNames;
	private readonly List<string> _generatedMethodDeclarations;
	private readonly IOpenApiPathItem _pathItem;
	private readonly OpenApiDocument _document;
	private readonly string _pathTemplate;

	public EndpointCodeResolver(
		string pathTemplate, 
		IOpenApiPathItem pathItem, 
		OpenApiDocument document, 
		List<string> generatedMethodNames,
		List<string> generatedMethodDeclarations)
	{
		_pathTemplate = pathTemplate;
		_pathItem = pathItem;
		_document = document;
		_components = document.Components ?? new OpenApiComponents();
		_generatedMethodNames = generatedMethodNames;
		_generatedMethodDeclarations = generatedMethodDeclarations;
	}
	
	protected override string Resolve()
	{
		var callsCode = _pathItem.Operations
			.Select(kvp => GenerateOperationCode(_pathTemplate, kvp.Key, kvp.Value))
			.Where(c => c is not null); // filter not generated code for not supported operations
		return string.Join("\n\n", callsCode);
	}
	
	private string? GenerateOperationCode(string pathTemplate, HttpMethod operationType, OpenApiOperation operation)
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

	private string GenerateCallCode(string pathTemplate, HttpMethod operationType, 
		OpenApiOperation operation, RequestDefinition? request, OpenApiMediaType? response)
	{
		var parameters = operation.Parameters?.ToArray() ?? [];
		var operationId = operation.OperationId;
		
		var methodName = GenerateMethodName();
		_generatedMethodNames.Add(methodName);
		var methodArguments = parameters
			.Select(GenerateMethodArgumentCode)
			.ToList();
		string responseType;
		string? generateResponseModelType;
		if (response is { Schema: not null } && !HttpMethod.Head.Equals(operationType))
		{
			var responseModelName = $"{methodName.Substring(0, methodName.Length - "Async".Length)}Result";
			var modelType = response.Schema.ToCsType(_document, out var generate, responseModelName, forceNullable: true);
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

		// Begin http request message generation
		var callCodeBuilder = new StringBuilder();
		callCodeBuilder.AppendLine($"""using var request = new HttpRequestMessage({httpMethod}, $"{preparedPathTemplate}");""");
		
		// Add code for header parameters
		var headerParameters = parameters.Where(p => p.In == ParameterLocation.Header).ToArray();
		foreach (var headerParameter in headerParameters)
			callCodeBuilder.AppendLine($"""request.Headers.Add("{headerParameter.Name}", {headerParameter.Name.NormalizeCsName(capitalizeFirst: false)});""");

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
			methodArguments.Insert(0, $"{request.Value.RequestType.Schema.ToCsType(_document, out var generate, bodyTypeName)} body");
			callCodeBuilder.AppendLine($"{"\t\t"}request.Content = JsonContent.Create(body, options: _jsonOptions);");
			generateRequestModelType = generate ? bodyTypeName : null;
		}
		else
		{
			generateRequestModelType = null;
		}
		
		// Generate code for sending request
		callCodeBuilder.AppendLine($"{"\t\t"}using var response = await _httpClient.SendAsync(request, cancellationToken);");
		var modelVariable = parameters.Any(p => p.Name == "model") ? "responseModel" : "model";
		if (response is { Schema: not null } && !HttpMethod.Head.Equals(operationType))
		{
			callCodeBuilder.AppendLine($"{"\t\t"}{response.Schema.ToCsType(_document, out _, generateResponseModelType, forceNullable: true)} {modelVariable};");
			callCodeBuilder.AppendLine($"{"\t\t"}if (response.IsSuccessStatusCode)");
			callCodeBuilder.AppendLine($"{"\t\t\t"}{modelVariable} = JsonSerializer.Deserialize<{response.Schema.ToCsType(_document, out _, generateResponseModelType)}>(await response.Content.ReadAsStreamAsync(cancellationToken), _jsonOptions);");
			callCodeBuilder.AppendLine($"{"\t\t"}else");
			callCodeBuilder.AppendLine($"{"\t\t\t"}{modelVariable} = default;");
		}
		
		var responseArguments = new List<string>
		{
			"response.IsSuccessStatusCode",
			"response.StatusCode"
		};
		if (response is { Schema: not null } && !HttpMethod.Head.Equals(operationType))
			responseArguments.Add(modelVariable);

		var methodDeclaration = $"public async Task<{responseType}> {methodName}({string.Join(", ", methodArguments.Append("CancellationToken cancellationToken = default"))})";
		_generatedMethodDeclarations.Add(methodDeclaration);
		
		var methodCode = 
			$$"""
			  {{"\t"}}{{methodDeclaration}}
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
			
			// build method name by path template and response type
			var descriptiveFragments = pathTemplate
				.Split(['/'], StringSplitOptions.RemoveEmptyEntries)
				.Where(f => !f.StartsWith("{") && !f.EndsWith("}"))
				.ToArray();
			
			// select method name fragments
			var methodNameFragments = new List<string?>();
			methodNameFragments.Add(descriptiveFragments.LastOrDefault()?.NormalizeCsName());
			if (descriptiveFragments.Length > 1)
			{
				methodNameFragments.Add("Of");
				methodNameFragments.AddRange(descriptiveFragments.Take(descriptiveFragments.Length - 1).Select(f => f.NormalizeCsName()));
			}
			
			// append with/by name extension by given operation parameters
			if (operation.Parameters?.Any() == true)
			{
				methodNameFragments.Add(HttpMethod.Get.Equals(operationType) || HttpMethod.Head.Equals(operationType) ? "By" : "With");
				methodNameFragments.AddRange(operation.Parameters.Select((p, i) => $"{(i == 0 ? null : "And")}{p.Name.NormalizeCsName()}"));
			}
			
			// generate iterated appendix when generated name is already existing
			var generatedMethodNameBase = $"{operationType.Method.ToLower().Capitalize()}{string.Concat(methodNameFragments.Where(f => !string.IsNullOrWhiteSpace(f)))}";
			int? appendix = null;
			while (_generatedMethodNames.Contains($"{generatedMethodNameBase}{appendix}Async"))
				appendix = (appendix ?? 1) + 1;
			
			return $"{generatedMethodNameBase}{appendix}Async";
		}

		string GenerateMethodArgumentCode(IOpenApiParameter parameter) => 
			$"{parameter.Schema.ToCsType(_document)} {parameter.Name.NormalizeCsName(false)}";

		string GeneratePreparedPathTemplate()
		{
			var queryParameters = parameters
				.Where(p => p.In == ParameterLocation.Query)
				.Select(p => p.Schema?.Type == JsonSchemaType.Array
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

			string GenerateParameterAssignment(IOpenApiParameter parameter, string memberName) =>
				parameter.Schema?.Type switch
				{
					JsonSchemaType.String when !parameter.Schema.Enum.Any()
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
					|| (s.Type == JsonSchemaType.Object && (s.Properties?.Any(p => p.Value.Format == "binary") ?? false)); // todo: use .Equals() ??
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
