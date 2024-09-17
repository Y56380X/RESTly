// Copyright Y56380X https://github.com/Y56380X/RESTly.
// Licensed under the MIT License.

namespace Restly;

internal static class StringExtensions
{
	public static string Capitalize(this string s) => s.Length > 0 
		? $"{char.ToUpper(s[0])}{s.Substring(1)}"
		: s;
	
	public static string NormalizeCsName(this string typeName) => string.Concat(
		typeName.Split('-', '_', '.', ' ').Select(Capitalize));
}
