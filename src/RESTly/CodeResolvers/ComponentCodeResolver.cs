// Copyright Y56380X https://github.com/Y56380X/RESTly.
// Licensed under the MIT License.

using System.Text;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Restly.CodeResolvers;

internal sealed class ComponentCodeResolver : CodeResolverBase
{
	private readonly string _modelTypeName;
	private readonly OpenApiSchema _schema;

	public ComponentCodeResolver(string modelTypeName, OpenApiSchema schema)
	{
		_modelTypeName = modelTypeName.NormalizeCsName();
		_schema = schema;
	}

	protected override string Resolve()
	{
		return _schema.Enum.Any() 
			? ResolveEnum()
			: ResolveModel();
	}

	private string ResolveEnum()
	{
		var enumCodeBuilder = new StringBuilder($"{"\t"}public enum {_modelTypeName}\n");
		enumCodeBuilder.AppendLine($"{"\t"}{{");
		enumCodeBuilder.AppendLine(string.Join(",\n", _schema.Enum.Select(ResolveEnumValue)));
		enumCodeBuilder.Append($"{"\t"}}}");

		return enumCodeBuilder.ToString();

		string? ResolveEnumValue(IOpenApiAny enumValue) => enumValue switch
		{
			OpenApiInteger oaEnumInteger => $"{"\t\t"}Value{oaEnumInteger.Value} = {oaEnumInteger.Value}",
			OpenApiString  oaEnumString  => $"{"\t\t"}{oaEnumString.Value.NormalizeCsName()}",
			_ => null // currently returns null; todo: give diagnostics info
		};
	}

	private string ResolveModel()
	{
		var baseModel = _schema.AllOf.SingleOrDefault(s => s.Reference != null);
		var modelProperties = _schema.Properties
			.Concat(baseModel?.Properties ?? new Dictionary<string, OpenApiSchema>())
			.Concat(_schema.AllOf.Where(s => s != baseModel).SelectMany(s => s.Properties))
			// only properties outside the base class should be JSON annotated
			.Select(p => GeneratePropertyCode(p, !(baseModel?.Properties.ContainsKey(p.Key) == true)))
			.ToArray();
		
		var propertyPrefix = modelProperties.Length > 1 ? "\n\t\t" : string.Empty;
		var inheritanceCode = baseModel?.Type == "object"
			? $"\n\t: {baseModel.Reference.Id.NormalizeCsName()}({string.Join(", ", baseModel.Properties.Select(p => GeneratePropertyName(p.Key)))})"
			: null;
		var modelCodeBuilder = new StringBuilder($"{"\t"}public record {_modelTypeName}({propertyPrefix}{string.Join(",\n\t\t", modelProperties)}){inheritanceCode};");
		
		// Generate sub models
		foreach (var property in _schema.Properties.Where(p => IsSubModel(p.Value)))
		{
			var subModelName = GenerateSubModelName(property.Key);
			var subModelSchema = property.Value.Type == "array"
				? property.Value.Items
				: property.Value;
			var codeResolver = new ComponentCodeResolver(subModelName, subModelSchema);
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

	private string GeneratePropertyName(string schemaName)
	{
		var normalizedPropertyName = schemaName.NormalizeCsName();
		return normalizedPropertyName == _modelTypeName
			? $"{normalizedPropertyName}_"
			: normalizedPropertyName;
	}
	
	private string GeneratePropertyCode(KeyValuePair<string, OpenApiSchema> property, bool withJsonAnnotation)
	{
		var propertyName = GeneratePropertyName(property.Key);

		var propertyType = IsSubModel(property.Value)
			? $"{GenerateSubModelName(property.Key)}{(property.Value.Type == "array" ? "[]" : string.Empty)}"
			: property.Value.ToCsType();

		var jsonPropertyName = withJsonAnnotation
			? $"[property:JsonPropertyName(\"{property.Key}\")]"
			: null;
		
		return $"{jsonPropertyName}{propertyType} {propertyName}";
	}
}