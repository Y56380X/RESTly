// Copyright Y56380X https://github.com/Y56380X/RESTly.
// Licensed under the MIT License.

using Microsoft.OpenApi.Models;

namespace Restly;

internal static class OpenApiExtensions
{
	private const int RecursionDepthLimit = 1; // todo: adjust max recursion depth and loop detection
	
	public static string ToCsType(this OpenApiSchema schema, out bool generate, string? generatedName = null, bool forceNullable = false, int stop = RecursionDepthLimit)
	{
		if (stop <= 0)
		{
			generate = false;
			return "object";
		}

		generate = false;
		var baseType = schema.Type switch
		{
			"string"  when schema.Enum.Any() 
			               && schema.Reference is not null   => schema.Reference.Id.NormalizeCsName(),
			"string"  when schema is { Format: "byte" }      => "byte[]",
			"string"  when schema is { Format: "uuid" }      => "Guid",
			"string"  when schema is { Format: "date-time" } => "DateTime",
			"string"                                         => "string",
			"integer" when schema.Enum.Any() 
			               && schema.Reference is not null   => schema.Reference.Id.NormalizeCsName(),
			"integer" when schema is { Format: "int64" }     => "long",
			"integer"                                        => "int",
			"number"  when schema is { Format: "float" }     => "float",
			"number"                                         => "double",
			"boolean"                                        => "bool",
			"array"                                          => $"{schema.Items.ToCsType(out generate, generatedName, false, stop)}[]",
			"object"  when schema.AdditionalProperties 
					       is {} propertiesSchema            => $"IDictionary<string, {propertiesSchema.ToCsType(out generate, generatedName, stop: stop)}>",
			_         when schema.Reference is {} reference
                           && ResolveReferenceSchema(reference) is {} referenceSchema
                           && !referenceSchema.Properties.Any()
				                                             => ResolveReferenceSchema(reference)?.ToCsType(out generate, generatedName, forceNullable, stop - 1) ?? reference.Id.NormalizeCsName(),
			_         when schema.Reference is {} reference  => reference.Id.NormalizeCsName(),
			_		  when schema.OneOf 
				           is { Count: > 0 } oneOf           => ResolveCommonBase(oneOf),
			_         when schema.Properties.Any()           => generatedName ?? "object",
			_                                                => "object"
		};
		generate = generate || (generatedName is not null && baseType.StartsWith(generatedName));
		
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
			// resolve only when there is just one base type (as only then it is the common one)
			// NOTE: currently only resolves in distance of 1
			return baseTypes.Length == 1 
				? baseTypes[0].NormalizeCsName() 
				: "object";
		}

		OpenApiSchema? ResolveReferenceSchema(OpenApiReference reference) =>
			reference.HostDocument?.Components.Schemas.TryGetValue(reference.Id, out var referenceSchema) ?? false
				? referenceSchema
				: null;
	}
	
	public static string ToCsType(this OpenApiSchema schema, bool forceNullable = false) => 
		ToCsType(schema, out _, forceNullable: forceNullable);
}