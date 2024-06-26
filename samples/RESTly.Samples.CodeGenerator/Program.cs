﻿using Microsoft.OpenApi.Readers;

var openApiReader = new OpenApiStringReader(new OpenApiReaderSettings
{
	ReferenceResolution = ReferenceResolutionSetting.ResolveLocalReferences,
	LoadExternalRefs = false
});
var document = openApiReader.Read(File.ReadAllText("simple-api.yaml"), out _);

var clientCodeResolver = new Restly.CodeResolvers.ClientCodeResolver(document);

Console.WriteLine(clientCodeResolver.GeneratedCode);
