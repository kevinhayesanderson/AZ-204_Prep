using Azure;
using Azure.Containers.ContainerRegistry;
using Azure.Identity;



// Get the service endpoint from the environment
Uri endpoint = new Uri(Environment.GetEnvironmentVariable("REGISTRY_ENDPOINT"));

// Create a new ContainerRegistryClient
ContainerRegistryClient client = new ContainerRegistryClient(endpoint, new DefaultAzureCredential());

// Get the collection of repository names from the registry
AsyncPageable<string> repositories = client.GetRepositoryNamesAsync();
await foreach (string repository in repositories)
{
    Console.WriteLine(repository);
}

//Handle Errors
string fakeRepositoryName = "doesnotexist";
client = new ContainerRegistryClient(endpoint, new DefaultAzureCredential());
ContainerRepository fakeRepository = client.GetRepository(fakeRepositoryName);

try
{
    await fakeRepository.GetPropertiesAsync();
}
catch (RequestFailedException ex) when (ex.Status == 404)
{
    Console.WriteLine("Repository wasn't found.");
    Console.WriteLine($"Service error: {ex.Message}.");
}