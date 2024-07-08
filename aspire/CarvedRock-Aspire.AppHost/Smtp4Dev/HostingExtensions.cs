// Put extensions in the Aspire.Hosting namespace to ease discovery as referencing
// the .NET Aspire hosting package automatically adds this namespace.
namespace Aspire.Hosting;
public static class Smtp4DevAspireHostingExtensions
{
    /// <summary>
     /// Adds the <see cref="Smtp4DevResource"/> to the given
     /// <paramref name="builder"/> instance. Uses the "latest" tag.
     /// </summary>
     /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
     /// <param name="name">The name of the resource.</param>
     /// <param name="httpPort">The HTTP port.</param>
     /// <param name="smtpPort">The SMTP port.</param>
     /// <returns>
     /// An <see cref="IResourceBuilder{Smtp4DevResource}"/> instance that
     /// represents the added Smtp4Dev resource.
     /// </returns>
    public static IResourceBuilder<Smtp4DevResource> AddSmtp4Dev(
        this IDistributedApplicationBuilder builder,
        string name,
        int? httpPort = null,
        int? smtpPort = null)
    {
        // The AddResource method is a core API within .NET Aspire and is
        // used by resource developers to wrap a custom resource in an
        // IResourceBuilder<T> instance. Extension methods to customize
        // the resource (if any exist) target the builder interface.
        var resource = new Smtp4DevResource(name);

        const string ImageName = "rnwood/smtp4dev";
        const string Registry = "docker.io";

        return builder.AddResource(resource)
                      .WithImage(ImageName)
                      .WithImageRegistry(Registry)
                      //.WithImageTag(MailDevContainerImageTags.Tag)
                      .WithHttpEndpoint(
                          targetPort: 80,
                          port: httpPort,
                          name: Smtp4DevResource.HttpEndpointName)
                      .WithEndpoint(
                          targetPort: 25,
                          port: smtpPort,
                          name: Smtp4DevResource.SmtpEndpointName);
    }
}
