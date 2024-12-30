using Microsoft.OpenApi.Reader;
using Microsoft.OpenApi.Readers;

var openApiReader = new OpenApiYamlReader();
var fileText = File.ReadAllText("./simple-api.yaml");
using var memory = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileText));
var readResult = await openApiReader.ReadAsync(
	memory,
	new OpenApiReaderSettings
	{
		ReferenceResolution = ReferenceResolutionSetting.ResolveLocalReferences,
		LoadExternalRefs = false
	});

var clientCodeResolver = new Restly.CodeResolvers.ClientCodeResolver(readResult.Document);

Console.WriteLine(clientCodeResolver.GeneratedCode);
