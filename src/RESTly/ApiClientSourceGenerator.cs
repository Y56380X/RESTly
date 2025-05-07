// Copyright Y56380X https://github.com/Y56380X/RESTly.
// Licensed under the MIT License.

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Reader;
using Microsoft.OpenApi.YamlReader;
using Restly.CodeResolvers;
using Restly.Models;

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
			try
			{
				AddApiClient(context, additionalTexts, clientDefinition, clientName);
			}
			catch (Exception exception)
			{
				var errorDiagnostics = Diagnostic.Create(new DiagnosticDescriptor(
					"RLY1000", "Error", $"Error during '{clientName}' client generation. ({exception})", "Error", DiagnosticSeverity.Error, true), 
					null);
				context.ReportDiagnostic(errorDiagnostics);
			}
		}
	}

	private static void AddApiClient(
		SourceProductionContext context, ImmutableArray<AdditionalText> additionalTexts,
		string? clientDefinition, string? clientName)
	{
		if (string.IsNullOrWhiteSpace(clientDefinition) || string.IsNullOrWhiteSpace(clientName))
			return; // todo: write analyzer message

		var definitionFile = additionalTexts.SingleOrDefault(a => a.Path.EndsWith(clientDefinition));
		if (definitionFile == null)
			return; // todo: write analyzer message

		var definitionContent = definitionFile.GetText()!.ToString();
		var extension = Path.GetExtension(clientDefinition);
		IOpenApiReader openApiReader = extension.Trim().ToLower() switch
		{
			".json"           => new OpenApiJsonReader(),
			".yaml" or ".yml" => new OpenApiYamlReader(),
			_ => throw new ArgumentOutOfRangeException()
		};
		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(definitionContent));
		var readResult = openApiReader.Read(stream, new OpenApiReaderSettings
		{
			LoadExternalRefs = false
		});
			
		if (readResult.Document is not {} apiSpecification)
			return;

		var generatedMethodDeclarations = new List<EndpointDefinition>();
		
		apiSpecification.Info.Title = clientName;
		
		var apiClientCode = GenerateApiClientCode(apiSpecification, generatedMethodDeclarations);
		context.AddSource($"{clientName}.g.cs", SourceText.From(apiClientCode, Encoding.UTF8));
		
		var apiClientInterface = GenerateApiClientInterface(apiSpecification, generatedMethodDeclarations);
		context.AddSource($"I{clientName}.g.cs", SourceText.From(apiClientInterface, Encoding.UTF8));
	}

	private static string GenerateApiClientCode(OpenApiDocument apiSpecification, List<EndpointDefinition> generatedMethodDeclarations)
	{
		var clientCodeResolver = new ClientCodeResolver(apiSpecification, generatedMethodDeclarations);
		return clientCodeResolver.GeneratedCode;
	}

	private static string GenerateApiClientInterface(OpenApiDocument apiSpecification, List<EndpointDefinition> generatedMethodDeclarations)
	{
		var clientInterfaceResolver = new ClientInterfaceResolver(apiSpecification, generatedMethodDeclarations);
		return clientInterfaceResolver.GeneratedCode;
	}
}