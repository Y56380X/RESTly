// Copyright Y56380X https://github.com/Y56380X/RESTly.
// Licensed under the MIT License.

using Microsoft.OpenApi.Models;

namespace Restly.CodeResolvers;

internal sealed class ModelCodeResolver : CodeResolverBase
{
	private readonly string _name;
	private readonly OpenApiSchema _schema;

	public ModelCodeResolver(string name, OpenApiSchema schema)
	{
		_name = name;
		_schema = schema;
	}
	
	protected override string Resolve()
	{
		var modelProperties = _schema.Properties
			.Select(PropertyCode);
		return $"{"\t"}public record {_name.Capitalize()}({string.Join(", ", modelProperties)});";

		string PropertyCode(KeyValuePair<string, OpenApiSchema> property) =>
			$"{property.Value.ToCsType()} {property.Key.Capitalize()}";
	}
}