using Azure;
using Azure.Containers.ContainerRegistry;
using Azure.Identity;




// Get the service endpoint from the environment
// Create a ContainerRegistryClient that will authenticate to your registry through Azure Active Directory
Uri endpoint = new Uri("https://myregistry.azurecr.io");
ContainerRegistryClient client = new ContainerRegistryClient(endpoint, new DefaultAzureCredential(),
    new ContainerRegistryClientOptions()
    {
        Audience = ContainerRegistryAudience.AzureResourceManagerPublicCloud
    });

// Get the collection of repository names from the registry
//Set artifact properties asynchronously
AsyncPageable<string> repositoryNames = client.GetRepositoryNamesAsync();
await foreach (string repositoryName in repositoryNames)
{
    ContainerRepository repository = client.GetRepository(repositoryName);

    // Obtain the images ordered from newest to oldest
    AsyncPageable<ArtifactManifestProperties> imageManifests =
        repository.GetAllManifestPropertiesAsync(manifestOrder: ArtifactManifestOrder.LastUpdatedOnDescending);

    // Delete images older than the first three.
    foreach (ArtifactManifestProperties imageManifest in imageManifests.ToBlockingEnumerable().Skip(3))
    {
        RegistryArtifact image = repository.GetArtifact(imageManifest.Digest);
        Console.WriteLine($"Deleting image with digest {imageManifest.Digest}.");
        Console.WriteLine($"Deleting the following tags from the image:");
        foreach (var tagName in imageManifest.Tags)
        {
            Console.WriteLine($"{imageManifest.RepositoryName}:{tagName}");
            await image.DeleteTagAsync(tagName);
        }
        await image.DeleteAsync();
    }
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