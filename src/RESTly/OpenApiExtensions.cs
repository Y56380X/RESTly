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
			"string"  when schema.Enum.Any() 
			              && schema.Reference is not null    => schema.Reference.Id.NormalizeCsName(),
			"string"  when schema is { Format: "byte" }      => "byte[]",
			"string"  when schema is { Format: "uuid" }      => "Guid",
			"string"  when schema is { Format: "date-time" } => "DateTime",
			"string"                                         => "string",
			"integer" when schema is { Format: "int64" }     => "long",
			"integer"                                        => "int",
			"number"  when schema is { Format: "float" }     => "float",
			"number"                                         => "double",
			"array"                                          => $"{schema.Items.ToCsType()}[]",
			"object"  when schema.AdditionalProperties 
					      is {} propertiesSchema             => $"IDictionary<string, {propertiesSchema.ToCsType()}>",
			_         when schema.Reference is {} reference  => reference.Id.NormalizeCsName(),
			_		  when schema.OneOf 
						  is { Count: > 0 } oneOf            => ResolveCommonBase(oneOf),
			_                                                => "object"
		};
		char? nullable = schema.Nullable || forceNullable ? '?' : null;
		return $"{baseType}{nullable}";

		string ResolveCommonBase(IEnumerable<OpenApiSchema> oneOf)
		{
			var schemas = oneOf.Select(s => s.Reference.HostDocument.Components.Schemas[s.Reference.Id]);
			var baseTypes = schemas
				.Select(s => s.AllOf.FirstOrDefault(a => a.Reference != null)?.Reference?.Id)
				.OfType<string>()
				.Distinct()
				.ToArray();
			// resolve only when there is just one base type (as only then is is the common one)
			// NOTE: currently only resolves in distance of 1
			return baseTypes.Length == 1 
				? baseTypes[0].NormalizeCsName() 
				: "object";
		}
	}
}