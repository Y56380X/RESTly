// Copyright Y56380X https://github.com/Y56380X/RESTly.
// Licensed under the MIT License.

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Restly;

[Generator]
public class FoundationSourceGenerator : IIncrementalGenerator
{
	private const string ClientAttributeCode =
		"""
		// <auto-generated/>
		namespace Restly;
		
		[System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
		public class RestlyClientAttribute : System.Attribute
		{
		    public RestlyClientAttribute(string clientDefinition, string clientName) { }
		}
		""";
	
	private const string ResponseClassesCode =
		"""
		// <auto-generated/>
		using System.Net;
		
		#nullable enable
		
		namespace Restly;
		
		public record Response(bool IsSuccessStatusCode, HttpStatusCode StatusCode);
	
		public record Response<T>(bool IsSuccessStatusCode, HttpStatusCode StatusCode, T? Model)
		        : Response(IsSuccessStatusCode, StatusCode);
		""";

	private const string HttpContentPolyfillCode =
		"""
		// <auto-generated/>
		using System.IO;
		using System.Net.Http;
		using System.Threading;
		using System.Threading.Tasks;

		#if NETSTANDARD2_0
		internal static class HttpContentPolyfill
		{
			public static Task<Stream> ReadAsStreamAsync(this HttpContent httpContent, CancellationToken _) => 
				httpContent.ReadAsStreamAsync();
		}
		#endif
		""";
	
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
			"RestlyClientAttribute.g.cs",
			SourceText.From(ClientAttributeCode, Encoding.UTF8)));
		
		context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
			"RestlyResponse.g.cs",
			SourceText.From(ResponseClassesCode, Encoding.UTF8)));
		
		context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
			"HttpContentPolyfill.g.cs",
			SourceText.From(HttpContentPolyfillCode, Encoding.UTF8)));
	}
}