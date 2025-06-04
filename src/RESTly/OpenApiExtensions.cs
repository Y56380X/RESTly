// Copyright Y56380X https://github.com/Y56380X/RESTly.
// Licensed under the MIT License.

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Microsoft.OpenApi.Models.References;

namespace Restly;

internal static class OpenApiExtensions
{
	private const int RecursionDepthLimit = 1; // todo: adjust max recursion depth and loop detection
	
	public static string ToCsType(this IOpenApiSchema schema, OpenApiDocument document, out bool generate, string? generatedName = null, bool forceNullable = false, int stop = RecursionDepthLimit)
	{
		if (stop <= 0)
		{
			generate = false;
			return "object";
		}

		generate = false;
		var resolvedSchemaType = schema.Type & ~JsonSchemaType.Null;
		var baseType = resolvedSchemaType switch
		{
			_ when schema.Enum?.Any() == true && schema is OpenApiSchemaReference { Reference.Id: {} referenceId } 
																		  => referenceId.NormalizeCsName(),
			JsonSchemaType.String  when schema is { Format: "byte" }      => "byte[]",
			JsonSchemaType.String  when schema is { Format: "uuid" }      => "Guid",
			JsonSchemaType.String  when schema is { Format: "date-time" } => "DateTime",
			JsonSchemaType.String                                         => "string",
			JsonSchemaType.Integer when schema is { Format: "int64" }     => "long",
			JsonSchemaType.Integer                                        => "int",
			JsonSchemaType.Number  when schema is { Format: "float" }     => "float",
			JsonSchemaType.Number                                         => "double",
			JsonSchemaType.Boolean                                        => "bool",
			JsonSchemaType.Array                                          => $"{schema.Items?.ToCsType(document, out generate, generatedName, false, stop) ?? "object"}[]",
			JsonSchemaType.Object  when schema.AdditionalProperties 
					       is {} propertiesSchema            => $"IDictionary<string, {propertiesSchema.ToCsType(document, out generate, generatedName, stop: stop)}>",
			_         when schema is OpenApiSchemaReference { Reference: {} reference }
                           && ResolveReferenceSchema(reference) is {} referenceSchema
                           && !referenceSchema.Properties.Any()
																// use reference schema id as type name, when it exists and is an `AllOf` schema
				                                             => referenceSchema.Type != null || (referenceSchema.AllOf?.Any() ?? false) == false || referenceSchema.Id == null
					                                             ? referenceSchema.ToCsType(document, out generate, generatedName, forceNullable, stop - 1) 
					                                             : reference.Id.NormalizeCsName(),
			_         when schema is OpenApiSchemaReference { Reference: {} reference }  
															 => reference.Id.NormalizeCsName(),
			_		  when schema.OneOf 
				           is { Count: > 0 } oneOf           => ResolveOneOf(oneOf),
			_		  when schema.AnyOf
						   is { Count: > 0 } anyOf           => ResolveAnyOf(anyOf),
			_         when schema.Properties?.Any() == true  => generatedName ?? "object",
			_                                                => "object"
		};
		generate = generate || (generatedName is not null && baseType.StartsWith(generatedName));
		
		char? nullable = schema.Type?.HasFlag(JsonSchemaType.Null) == true || forceNullable ? '?' : null;
		return $"{baseType}{nullable}";

		string ResolveAnyOf(IList<IOpenApiSchema> anyOf)
		{
			var possiblyNullable = anyOf.Any(s => s.Type == JsonSchemaType.Null);
			var typeCandidates = anyOf.Where(s => s.Type != JsonSchemaType.Null).ToArray();
			
			return typeCandidates.Length == 1 // NOTE: currently just resolve when there is just one other type than `null`
				? typeCandidates.Single().ToCsType(document, out _, forceNullable: possiblyNullable)
				: "object";
		}

		string ResolveOneOf(IEnumerable<IOpenApiSchema> oneOf)
		{
			var allSchemas = document.Components?.Schemas ?? new Dictionary<string, IOpenApiSchema>();
			var schemas = oneOf.OfType<OpenApiSchemaReference>().Select(s => allSchemas[s.Reference.Id]);
			var baseTypes = schemas
				.Select(s => s.AllOf?.OfType<OpenApiSchemaReference>().FirstOrDefault(a => a.Reference != null)?.Reference?.Id)
				.OfType<string>()
				.Distinct()
				.ToArray();
			// resolve only when there is just one base type (as only then it is the common one)
			// NOTE: currently only resolves in distance of 1
			return baseTypes.Length == 1 
				? baseTypes[0].NormalizeCsName() 
				: "object";
		}

		IOpenApiSchema? ResolveReferenceSchema(OpenApiReference reference) =>
			document.Components?.Schemas?.TryGetValue(reference.Id, out var referenceSchema) ?? false
				? referenceSchema
				: null;
	}
	
	public static string ToCsType(this IOpenApiSchema schema, OpenApiDocument document, bool forceNullable = false) => 
		ToCsType(schema, document, out _, forceNullable: forceNullable);
}