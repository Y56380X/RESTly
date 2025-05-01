using Microsoft.OpenApi.Models;

namespace Restly.Models;

public struct EndpointDefinition
{
	public string           MethodDeclaration { get; }
	public OpenApiOperation SpecOperation     { get; }

	public EndpointDefinition(string methodDeclaration, OpenApiOperation specOperation)
	{
		MethodDeclaration = methodDeclaration;
		SpecOperation = specOperation;
	}
}