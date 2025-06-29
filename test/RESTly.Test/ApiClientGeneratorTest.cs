using Microsoft.OpenApi.Reader;
using Microsoft.OpenApi.YamlReader;
using Restly.Models;

namespace RESTly.Test;

public class Tests
{
	[Test]
	public async Task TestSimpleApi()
	{
		var openApiReader = new OpenApiJsonReader();
		var fileText = await File.ReadAllTextAsync("./Data/simple-api.json");
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

	[Test]
	public async Task TestOVH_CloudV1()
	{
		var openApiReader = new OpenApiJsonReader();
		using var httpClient = new HttpClient();
		var readResult = await openApiReader.ReadAsync(
			await httpClient.GetStreamAsync("https://eu.api.ovh.com/v1/cloud.json?format=openapi3"),
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
	public async Task TestOpenAI()
	{
		using var httpClient = new HttpClient();
		await using var specStream = await httpClient.GetStreamAsync(
			"https://raw.githubusercontent.com/openai/openai-openapi/refs/heads/master/openapi.yaml");
		var readResult = await ReadOpenApiYamlAsync(specStream);

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
	public async Task TestOpenProjectJson()
	{
		using var httpClient = new HttpClient();
		await using var specStream = await httpClient.GetStreamAsync("https://www.openproject.org/docs/api/v3/spec.json");
		var readResult = await ReadOpenApiJsonAsync(specStream);

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
	public async Task TestOpenProjectYaml()
	{
		using var httpClient = new HttpClient();
		await using var specStream = await httpClient.GetStreamAsync("https://www.openproject.org/docs/api/v3/spec.yml");
		var readResult = await ReadOpenApiYamlAsync(specStream);

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
	public async Task TestMittwald()
	{
		using var httpClient = new HttpClient();
		await using var specStream = await httpClient.GetStreamAsync("https://api.mittwald.de/v2/openapi.json");
		var readResult = await ReadOpenApiJsonAsync(specStream);

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
	public async Task TestIonosAuth()
	{
		using var httpClient = new HttpClient();
		await using var specStream = await httpClient.GetStreamAsync("https://api.ionos.com/docs/public-authentication-v1.ga.yaml");
		var readResult = await ReadOpenApiYamlAsync(specStream);

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
	public async Task TestIonosCloud()
	{
		using var httpClient = new HttpClient();
		await using var specStream = await httpClient.GetStreamAsync("https://api.ionos.com/docs/public-cloud-v6.ga.yaml");
		var readResult = await ReadOpenApiYamlAsync(specStream);

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
	public async Task TestIonosCdn()
	{
		using var httpClient = new HttpClient();
		await using var specStream = await httpClient.GetStreamAsync("https://api.ionos.com/docs/public-cdn-v1.ga.yml");
		var readResult = await ReadOpenApiYamlAsync(specStream);

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
	public async Task TestIonosContainerRegistry()
	{
		using var httpClient = new HttpClient();
		await using var specStream = await httpClient.GetStreamAsync("https://api.ionos.com/docs/public-containerregistry-v1.ga.yml");
		var readResult = await ReadOpenApiYamlAsync(specStream);

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
	public async Task TestIonosVpnGateway()
	{
		using var httpClient = new HttpClient();
		await using var specStream = await httpClient.GetStreamAsync("https://api.ionos.com/docs/public-vpn-v1.ga.yml");
		var readResult = await ReadOpenApiYamlAsync(specStream);

		var generatedMethodDeclarations = new List<EndpointDefinition>();

		var clientCode = new Restly.CodeResolvers.ClientCodeResolver(readResult.Document, generatedMethodDeclarations).GeneratedCode;
		var interfaceCode = new Restly.CodeResolvers.ClientInterfaceResolver(readResult.Document, generatedMethodDeclarations).GeneratedCode;
		
		Assert.Multiple(() =>
		{
			Assert.That(clientCode, Is.Not.Empty);
			Assert.That(interfaceCode, Is.Not.Empty);
		});
	}
	
	#region Helper Methods

	private static async Task<ReadResult> ReadOpenApiJsonAsync(Stream stream)
	{
		var openApiReader = new OpenApiJsonReader();
		var readResult = await openApiReader.ReadAsync(
			stream,
			new OpenApiReaderSettings
			{
				LoadExternalRefs = false
			});
		return readResult;
	}

	private static async Task<ReadResult> ReadOpenApiYamlAsync(Stream stream)
	{
		var openApiReader = new OpenApiYamlReader();
		var readResult = await openApiReader.ReadAsync(
			stream,
			new OpenApiReaderSettings
			{
				LoadExternalRefs = false
			});
		return readResult;
	}

	#endregion
}