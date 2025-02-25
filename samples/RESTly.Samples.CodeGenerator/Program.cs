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

var generatedMethodDeclarations = new List<string>();

var clientCodeResolver = new Restly.CodeResolvers.ClientCodeResolver(readResult.Document, generatedMethodDeclarations);
Console.WriteLine(clientCodeResolver.GeneratedCode);

var clientInterfaceResolver = new Restly.CodeResolvers.ClientInterfaceResolver(readResult.Document, generatedMethodDeclarations);
Console.WriteLine(clientInterfaceResolver.GeneratedCode);
