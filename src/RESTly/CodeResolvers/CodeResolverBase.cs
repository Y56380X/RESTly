// Copyright Y56380X https://github.com/Y56380X/RESTly.
// Licensed under the MIT License.

using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("Restly.Samples.CodeGenerator")]
[assembly:InternalsVisibleTo("Restly.Test")]

namespace Restly.CodeResolvers;

internal abstract class CodeResolverBase
{
	private string? _generatedCode;
	public string GeneratedCode => _generatedCode ??= Resolve();

	protected static string GeneratedCodeAttribute =>
		$"[global::System.CodeDom.Compiler.GeneratedCode(\"RESTly\", \"{typeof(CodeResolverBase).Assembly.GetName().Version}\")]";
	
	protected abstract string Resolve();
}