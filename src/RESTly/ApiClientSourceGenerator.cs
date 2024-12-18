// Copyright Y56380X https://github.com/Y56380X/RESTly.
// Licensed under the MIT License.

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Restly.CodeResolvers;

namespace Restly;

[Generator(LanguageNames.CSharp)]
public class ApiClientSourceGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// collect RESTly client attributes and possible files for OpenApi specifications
		var generationBase = context.CompilationProvider
			.Select((c, _) => c.Assembly)
			.SelectMany((s, _) => s.GetAttributes()
				.Where(a => a.AttributeClass?.Name.Equals("RestlyClientAttribute") ?? false))
			.Collect()
			.Combine(context.AdditionalTextsProvider.Where(a => a.Path.EndsWith(".json") || a.Path.EndsWith(".yml") || a.Path.EndsWith(".yaml")).Collect());
		
		context.RegisterSourceOutput(generationBase, Generator);
	}

	private static void Generator(SourceProductionContext context, (ImmutableArray<AttributeData>, ImmutableArray<AdditionalText>) generationBase)
	{
		var (attributes, additionalTexts) = (generationBase.Item1, generationBase.Item2);

		// Generate API client code based on the given definition file names of the assembly attributes
		var clientDefinitions = attributes.Select(a => (
			Definition: a.ConstructorArguments[0].Value as string, 
			Name      : a.ConstructorArguments[1].Value as string));
		foreach (var (clientDefinition, clientName) in clientDefinitions)
		{
			if (string.IsNullOrWhiteSpace(clientDefinition) || string.IsNullOrWhiteSpace(clientName))
				return; // todo: write analyzer message

			var definitionFile = additionalTexts.SingleOrDefault(a => a.Path.EndsWith(clientDefinition));
			if (definitionFile == null)
				return; // todo: write analyzer message

			var definitionContent = definitionFile.GetText()!.ToString();
			var openApiReader = new OpenApiStringReader(new OpenApiReaderSettings
			{
				ReferenceResolution = ReferenceResolutionSetting.ResolveLocalReferences,
				LoadExternalRefs = false
			});
			var apiSpecification = openApiReader.Read(definitionContent, out _);
			apiSpecification.Info.Title = clientName;
			var apiClientCode = GenerateApiClientCode(apiSpecification);
			context.AddSource($"{clientName}.g.cs", SourceText.From(apiClientCode, Encoding.UTF8));
		}
	}

	private static string GenerateApiClientCode(OpenApiDocument apiSpecification)
	{
		var clientCodeResolver = new ClientCodeResolver(apiSpecification);
		return clientCodeResolver.GeneratedCode;
	}
}