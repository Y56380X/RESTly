// Copyright Y56380X https://github.com/Y56380X/RESTly.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
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
		var enumCodeBuilder = new StringBuilder();
		if (_schema.Description is { } enumDescription)
		{
			enumCodeBuilder.AppendLine($"{'\t'}/// <summary>");
			foreach (var descriptionPart in enumDescription.Split(['\n'], StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()))
				enumCodeBuilder.AppendLine($"{'\t'}///\t{descriptionPart}");
			enumCodeBuilder.AppendLine($"{'\t'}/// </summary>");
		}
		enumCodeBuilder.AppendLine($"{"\t"}public enum {_modelTypeName}");
		enumCodeBuilder.AppendLine($"{"\t"}{{");
		enumCodeBuilder.AppendLine(string.Join(",\n", _schema.Enum.Select(ResolveEnumValue)));
		enumCodeBuilder.Append($"{"\t"}}}");

		return enumCodeBuilder.ToString();

		string? ResolveEnumValue(JsonNode enumValue)
		{
			var kind = enumValue.GetValueKind();
			var (enumValueCode, realValueString) = kind switch
			{
				JsonValueKind.Number
					=> ($"{"\t\t"}Value{enumValue.GetValue<int>()} = {enumValue.GetValue<int>()}", enumValue.GetValue<int>().ToString()),
				JsonValueKind.String when int.TryParse(enumValue.GetValue<string>(), out var intFromString)
					=> ($"{"\t\t"}Value{intFromString} = {intFromString}", enumValue.GetValue<string>()),
				JsonValueKind.String
					=> ($"{"\t\t"}{enumValue.GetValue<string>().NormalizeCsName()}", enumValue.GetValue<string>()),
				JsonValueKind.Null
					=> ($"{"\t\t"}Null", null),
				_   => ($"{"\t\t"}{enumValue.ToJsonString().NormalizeCsName()}", enumValue.ToJsonString())
			};
			
			return enumValueCode.Trim() == realValueString 
				? enumValueCode 
				: $"{"\t\t"}[JsonStringEnumMemberName(\"{realValueString}\")]\n{enumValueCode}";
		}
	}

	private string ResolveModel()
	{
		var baseModel = _schema.AllOf.FirstOrDefault(s => s.Reference != null);
		var modelProperties = _schema.Properties
			.Concat(baseModel?.Properties ?? new Dictionary<string, OpenApiSchema>())
			.Concat(_schema.AllOf.Where(s => s != baseModel).SelectMany(s => s.Properties))
			.ToArray();
		var modelPropertyCodeFragments = modelProperties
			// only properties outside the base class should be JSON annotated
			.Select(p => GeneratePropertyCode(p, baseModel?.Properties.ContainsKey(p.Key) != true))
			.ToArray();

		var derivedTypes = _schema.Reference?.HostDocument?.Components.Schemas
			.Where(s1 => s1.Value.AllOf.Any(s2 => s2.Equals(_schema)))
			.ToArray() ?? [];
		
		var propertyPrefix = modelProperties.Length > 1 ? "\n\t\t" : string.Empty;
		var inheritanceCode = baseModel?.Type == JsonSchemaType.Object
			? $"\n\t: {baseModel.Reference.Id.NormalizeCsName()}({string.Join(", ", baseModel.Properties.Select(p => GeneratePropertyName(p.Key)))})"
			: null;

		var modelCodeBuilder = new StringBuilder();
		
		// Generate XML annotation
		if (_schema.Description is { } modelDescription)
		{
			modelCodeBuilder.AppendLine($"{'\t'}/// <summary>");
			foreach (var descriptionPart in modelDescription.Split(['\n'], StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()))
				modelCodeBuilder.AppendLine($"{'\t'}///\t{descriptionPart}");
			modelCodeBuilder.AppendLine($"{'\t'}/// </summary>");
		}
		foreach (var modelProperty in modelProperties.Where(p => !string.IsNullOrWhiteSpace(p.Value.Description)))
		{
			modelCodeBuilder.AppendLine($"""{'\t'}/// <param name="{GeneratePropertyName(modelProperty.Key)}">""");
			foreach (var descriptionPart in modelProperty.Value.Description.Split(['\n'], StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()))
				modelCodeBuilder.AppendLine($"{'\t'}///\t{descriptionPart}");
			modelCodeBuilder.AppendLine($"{'\t'}/// </param>");
		}
		
		// Add json polymorphic information when component has derived types
		if (derivedTypes.Length > 0)
			modelCodeBuilder.AppendLine($"{'\t'}[JsonPolymorphic]");
		foreach (var derivedType in derivedTypes)
			modelCodeBuilder.AppendLine($"{'\t'}[JsonDerivedType(typeof({derivedType.Key.NormalizeCsName()}), nameof({derivedType.Key.NormalizeCsName()}))]");

		modelCodeBuilder.Append($"{"\t"}public record {_modelTypeName}({propertyPrefix}{string.Join(",\n\t\t", modelPropertyCodeFragments)}){inheritanceCode};");
		
		// Generate sub models
		foreach (var property in _schema.Properties.Where(p => IsSubModel(p.Value)))
		{
			var subModelName = GenerateSubModelName(property.Key);
			var subModelSchema = property.Value.Type == JsonSchemaType.Array
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
		schema is { Type: JsonSchemaType.Object, Reference: null } 
			   or { Type: JsonSchemaType.Array, Items: { Type: JsonSchemaType.Object, Reference: null } };

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
			? $"{GenerateSubModelName(property.Key)}{(property.Value.Type == JsonSchemaType.Array ? "[]" : string.Empty)}"
			: property.Value.ToCsType();

		var jsonPropertyName = withJsonAnnotation
			? $"[property:JsonPropertyName(\"{property.Key}\")]"
			: null;
		
		return $"{jsonPropertyName}{propertyType} {propertyName}";
	}
}