// Copyright Y56380X https://github.com/Y56380X/RESTly.
// Licensed under the MIT License.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Restly;

[Generator]
public class ApiClientSourceGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// collect RESTly client attributes and possible files for OpenApi specifications
		var generationBases = context.CompilationProvider
			.Select((c, _) => c.Assembly)
			.SelectMany((s, _) => s.GetAttributes()
				.Where(a => a.AttributeClass?.Name.Equals("RestlyClientAttribute") ?? false))
			.Collect()
			.Combine(context.AdditionalTextsProvider.Where(a => a.Path.EndsWith(".json") || a.Path.EndsWith(".yml") || a.Path.EndsWith(".yaml")).Collect());
		
		context.RegisterSourceOutput(generationBases, Generator);
	}

	private void Generator(SourceProductionContext context, (ImmutableArray<AttributeData>, ImmutableArray<AdditionalText>) generationBase)
	{
		throw new NotImplementedException();
	}
}