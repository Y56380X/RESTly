using Microsoft.OpenApi.Reader;
using Microsoft.OpenApi.YamlReader;
using Restly.Models;

namespace RESTly.Test;

public class Tests
{
	[Test]
	public async Task TestSimpleApi()
	{
		var openApiReader = new OpenApiYamlReader();
		var fileText = await File.ReadAllTextAsync("./Data/simple-api.yaml");
		using var memory = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileText));
		var readResult = await openApiReader.ReadAsync(
			memory,
			new OpenApiReaderSettings
			{
				LoadExternalRefs = false
			});

		var generatedMethodDeclarations = new List<EndpointDefinition>();

		var clientCode = new Restly.CodeResolvers.ClientCodeResolver(readResult.Document, generatedMethodDeclarations).GeneratedCode;
		var interfaceCode = new Restly.CodeResolvers.ClientInterfaceResolver(readResult.Document, generatedMethodDeclarations).GeneratedCode;
		
		Assert.Multiple(() =>
		{
			Assert.That(clientCode, Is.Not.Empty);
			Assert.That(interfaceCode, Is.Not.Empty);
		});
	}

	[Test]
	public async Task TestHetznerApi()
	{
		var openApiReader = new OpenApiJsonReader();
		using var httpClient = new HttpClient();
		var readResult = await openApiReader.ReadAsync(
			await httpClient.GetStreamAsync("https://docs.hetzner.cloud/spec.json"),
			new OpenApiReaderSettings
			{
				LoadExternalRefs = false
			});

		var generatedMethodDeclarations = new List<EndpointDefinition>();

		var clientCode = new Restly.CodeResolvers.ClientCodeResolver(readResult.Document, generatedMethodDeclarations).GeneratedCode;
		var interfaceCode = new Restly.CodeResolvers.ClientInterfaceResolver(readResult.Document, generatedMethodDeclarations).GeneratedCode;
		
		Assert.Multiple(() =>
		{
			Assert.That(clientCode, Is.Not.Empty);
			Assert.That(interfaceCode, Is.Not.Empty);
		});
	}

	[Test]
	public async Task TestUnofficialHetznerApi()
	{
		var openApiReader = new OpenApiJsonReader();
		using var httpClient = new HttpClient();
		var readResult = await openApiReader.ReadAsync(
			await httpClient.GetStreamAsync("https://raw.githubusercontent.com/MaximilianKoestler/hcloud-openapi/refs/heads/main/openapi/hcloud.json"),
			new OpenApiReaderSettings
			{
				LoadExternalRefs = false
			});

		var generatedMethodDeclarations = new List<EndpointDefinition>();

		var clientCode = new Restly.CodeResolvers.ClientCodeResolver(readResult.Document, generatedMethodDeclarations).GeneratedCode;
		var interfaceCode = new Restly.CodeResolvers.ClientInterfaceResolver(readResult.Document, generatedMethodDeclarations).GeneratedCode;
		
		Assert.Multiple(() =>
		{
			Assert.That(clientCode, Is.Not.Empty);
			Assert.That(interfaceCode, Is.Not.Empty);
		});
	}

	[Test]
	public async Task TestKeycloakApiJson()
	{
		var openApiReader = new OpenApiJsonReader();
		using var httpClient = new HttpClient();
		var readResult = await openApiReader.ReadAsync(
			await httpClient.GetStreamAsync("https://www.keycloak.org/docs-api/latest/rest-api/openapi.json"),
			new OpenApiReaderSettings
			{
				LoadExternalRefs = false
			});

		var generatedMethodDeclarations = new List<EndpointDefinition>();

		var clientCode = new Restly.CodeResolvers.ClientCodeResolver(readResult.Document, generatedMethodDeclarations).GeneratedCode;
		var interfaceCode = new Restly.CodeResolvers.ClientInterfaceResolver(readResult.Document, generatedMethodDeclarations).GeneratedCode;
		
		Assert.Multiple(() =>
		{
			Assert.That(clientCode, Is.Not.Empty);
			Assert.That(interfaceCode, Is.Not.Empty);
		});
	}

	[Test]
	public async Task TestKeycloakApiYaml()
	{
		var openApiReader = new OpenApiYamlReader();
		using var httpClient = new HttpClient();
		var readResult = await openApiReader.ReadAsync(
			await httpClient.GetStreamAsync("https://www.keycloak.org/docs-api/latest/rest-api/openapi.yaml"),
			new OpenApiReaderSettings
			{
				LoadExternalRefs = false
			});

		var generatedMethodDeclarations = new List<EndpointDefinition>();

		var clientCode = new Restly.CodeResolvers.ClientCodeResolver(readResult.Document, generatedMethodDeclarations).GeneratedCode;
		var interfaceCode = new Restly.CodeResolvers.ClientInterfaceResolver(readResult.Document, generatedMethodDeclarations).GeneratedCode;
		
		Assert.Multiple(() =>
		{
			Assert.That(clientCode, Is.Not.Empty);
			Assert.That(interfaceCode, Is.Not.Empty);
		});
	}

	[Test]
	public async Task TestDeepL()
	{
		var openApiReader = new OpenApiYamlReader();
		using var httpClient = new HttpClient();
		var readResult = await openApiReader.ReadAsync(
			await httpClient.GetStreamAsync("https://raw.githubusercontent.com/DeepLcom/openapi/main/openapi_gitbook.yaml"),
			new OpenApiReaderSettings
			{
				LoadExternalRefs = false
			});

		var generatedMethodDeclarations = new List<EndpointDefinition>();

		var clientCode = new Restly.CodeResolvers.ClientCodeResolver(readResult.Document, generatedMethodDeclarations).GeneratedCode;
		var interfaceCode = new Restly.CodeResolvers.ClientInterfaceResolver(readResult.Document, generatedMethodDeclarations).GeneratedCode;
		
		Assert.Multiple(() =>
		{
			Assert.That(clientCode, Is.Not.Empty);
			Assert.That(interfaceCode, Is.Not.Empty);
		});
	}

	[Test]
	public async Task TestBlackForestLabs()
	{
		var openApiReader = new OpenApiJsonReader();
		using var httpClient = new HttpClient();
		var readResult = await openApiReader.ReadAsync(
			await httpClient.GetStreamAsync("https://api.us1.bfl.ai/openapi.json"),
			new OpenApiReaderSettings
			{
				LoadExternalRefs = false
			});

		var generatedMethodDeclarations = new List<EndpointDefinition>();

		var clientCode = new Restly.CodeResolvers.ClientCodeResolver(readResult.Document, generatedMethodDeclarations).GeneratedCode;
		var interfaceCode = new Restly.CodeResolvers.ClientInterfaceResolver(readResult.Document, generatedMethodDeclarations).GeneratedCode;
		
		Assert.Multiple(() =>
		{
			Assert.That(clientCode, Is.Not.Empty);
			Assert.That(interfaceCode, Is.Not.Empty);
		});
	}

	[Test]
	public async Task TestMistralAi()
	{
		var openApiReader = new OpenApiYamlReader();
		using var httpClient = new HttpClient();
		var readResult = await openApiReader.ReadAsync(
			await httpClient.GetStreamAsync("https://docs.mistral.ai/redocusaurus/plugin-redoc-0.yaml"),
			new OpenApiReaderSettings
			{
				LoadExternalRefs = false
			});

		var generatedMethodDeclarations = new List<EndpointDefinition>();

		var clientCode = new Restly.CodeResolvers.ClientCodeResolver(readResult.Document, generatedMethodDeclarations).GeneratedCode;
		var interfaceCode = new Restly.CodeResolvers.ClientInterfaceResolver(readResult.Document, generatedMethodDeclarations).GeneratedCode;
		
		Assert.Multiple(() =>
		{
			Assert.That(clientCode, Is.Not.Empty);
			Assert.That(interfaceCode, Is.Not.Empty);
		});
	}
}