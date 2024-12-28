// Copyright Y56380X https://github.com/Y56380X/RESTly.
// Licensed under the MIT License.

namespace Restly;

internal static class StringExtensions
{
	public static string Capitalize(this string s) => s.Length > 0 
		? $"{char.ToUpper(s[0])}{s.Substring(1)}"
		: s;
	
	public static string Lowerize(this string s) => s.Length > 0 
		? $"{char.ToLower(s[0])}{s.Substring(1)}"
		: s;
	
	public static string NormalizeCsName(this string typeName, bool capitalizeFirst = true) => string.Concat(
		typeName.Split('-', '_', '.', ' ', '/', '\\').Select((f, i) => !capitalizeFirst && i == 0 ? Lowerize(f) : Capitalize(f)));
}
