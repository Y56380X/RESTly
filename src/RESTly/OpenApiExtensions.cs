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
			"string" when schema is { Format: "byte" }      => "byte[]",
			"string" when schema is { Format: "uuid" }      => "Guid",
			"string" when schema is { Format: "date-time" } => "DateTime",
			"string"                                        => "string",
			"integer" when schema is { Format: "int64" }    => "long",
			"integer"                                       => "int",
			"array"                                         => $"{schema.Items.ToCsType()}[]",
			"object" when schema.Reference is {} reference  => reference.Id.NormalizeCsName(),
			"object" when schema.AdditionalProperties 
					 is {} propertiesSchema                 => $"IDictionary<string, {propertiesSchema.ToCsType()}>",
			_                                               => "object"
		};
		char? nullable = schema.Nullable || forceNullable ? '?' : null;
		return $"{baseType}{nullable}";
	}
}