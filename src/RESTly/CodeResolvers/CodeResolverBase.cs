// Copyright Y56380X https://github.com/Y56380X/RESTly.
// Licensed under the MIT License.

namespace Restly.CodeResolvers;

internal abstract class CodeResolverBase
{
	private string? _generatedCode;
	public string GeneratedCode => _generatedCode ??= Resolve();
	
	protected abstract string Resolve();
}