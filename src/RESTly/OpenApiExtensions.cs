// Copyright Y56380X https://github.com/Y56380X/RESTly.
// Licensed under the MIT License.

using Microsoft.OpenApi.Models;

namespace Restly;

internal static class OpenApiExtensions
{
	public static string ToCsType(this OpenApiSchema schema, bool forceNullable = false)
	{
		var baseType = schema.Type switch
		{
			"string" when schema is { Format: "uuid" } => "Guid",
			"string" => "string",
			"array"  => $"{schema.Items.ToCsType()}[]",
			_        => "object"
		};
		char? nullable = schema.Nullable || forceNullable ? '?' : null;
		return $"{baseType}{nullable}";
	}
}