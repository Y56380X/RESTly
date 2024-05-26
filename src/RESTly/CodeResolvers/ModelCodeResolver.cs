// Copyright Y56380X https://github.com/Y56380X/RESTly.
// Licensed under the MIT License.

using System.Text;
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
		var modelProperties = _schema.Properties.Select(GeneratePropertyCode).ToArray();
		var propertyPrefix = modelProperties.Length > 1 ? "\n\t\t" : string.Empty;
		var modelCodeBuilder = new StringBuilder($"{"\t"}public record {_modelTypeName}({propertyPrefix}{string.Join(",\n\t\t", modelProperties)});");
		
		// Generate sub models
		foreach (var property in _schema.Properties.Where(p => IsSubModel(p.Value)))
		{
			var subModelName = GenerateSubModelName(property.Key);
			var subModelSchema = property.Value.Type == "array"
				? property.Value.Items
				: property.Value;
			var codeResolver = new ModelCodeResolver(subModelName, subModelSchema);
			modelCodeBuilder.AppendLine();
			modelCodeBuilder.AppendLine();
			modelCodeBuilder.Append(codeResolver.Resolve());
		}
		
		return modelCodeBuilder.ToString();
	}

	private string GenerateSubModelName(string schemaName) => $"{_modelTypeName}{schemaName.Capitalize()}".NormalizeCsName();

	private static bool IsSubModel(OpenApiSchema schema) => 
		schema is { Type: "object", Reference: null } 
			   or { Type: "array", Items: { Type: "object", Reference: null } };

	private string GeneratePropertyCode(KeyValuePair<string, OpenApiSchema> property)
	{
		var normalizedPropertyName = property.Key.NormalizeCsName();
		var selectedPropertyName = normalizedPropertyName == _modelTypeName
			? $"{normalizedPropertyName}_"
			: normalizedPropertyName;

		var propertyType = IsSubModel(property.Value)
			? $"{GenerateSubModelName(property.Key)}{(property.Value.Type == "array" ? "[]" : string.Empty)}"
			: property.Value.ToCsType();
		
		return $"[property:JsonPropertyName(\"{property.Key}\")]{propertyType} {selectedPropertyName}";
	}
}