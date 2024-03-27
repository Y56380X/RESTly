// Copyright Y56380X https://github.com/Y56380X/RESTly.
// Licensed under the MIT License.

using Microsoft.OpenApi.Models;

namespace Restly.CodeResolvers;

public class ModelCodeResolver
{
	private readonly string _name;
	private readonly OpenApiSchema _schema;
	
	private string? _generatedCode;
	public string GeneratedCode => _generatedCode ??= Resolve();

	public ModelCodeResolver(string name, OpenApiSchema schema)
	{
		_name = name;
		_schema = schema;
	}
	
	private string Resolve()
	{
		var modelProperties = _schema.Properties
			.Select(PropertyCode);
		return $"{"\t"}public record {_name.Capitalize()}({string.Join(", ", modelProperties)});";

		string PropertyCode(KeyValuePair<string, OpenApiSchema> property) =>
			$"{property.Value.ToCsType()} {property.Key.Capitalize()}";
	}
}