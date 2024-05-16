// Copyright Y56380X https://github.com/Y56380X/RESTly.
// Licensed under the MIT License.

using Microsoft.OpenApi.Models;

namespace Restly.CodeResolvers;

internal sealed class ModelCodeResolver : CodeResolverBase
{
	private readonly string _modelTypeName;
	private readonly OpenApiSchema _schema;

	public ModelCodeResolver(string modelTypeName, OpenApiSchema schema)
	{
		_modelTypeName = modelTypeName.NormalizeCsName();
		_schema = schema;
	}
	
	protected override string Resolve()
	{
		var modelProperties = _schema.Properties.Select(GeneratePropertyCode);
		return $"{"\t"}public record {_modelTypeName}({string.Join(",\n\t\t", modelProperties)});";
	}

	private string GeneratePropertyCode(KeyValuePair<string, OpenApiSchema> property)
	{
		var normalizedPropertyName = property.Key.NormalizeCsName();
		var selectedPropertyName = normalizedPropertyName == _modelTypeName
			? $"{normalizedPropertyName}_"
			: normalizedPropertyName;
		
		return $"[property:JsonPropertyName(\"{property.Key}\")]{property.Value.ToCsType()} {selectedPropertyName}";
	}
}