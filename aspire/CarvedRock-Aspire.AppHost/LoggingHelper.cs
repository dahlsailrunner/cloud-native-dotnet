namespace Aspire.Hosting;

internal static class LoggingHelper
{
    internal static IResourceBuilder<T> WithSharedLoggingLevels<T>(this IResourceBuilder<T> builder)
        where T : IResourceWithEnvironment
    {
        var dict = new Dictionary<string, string>
        {
            { "Default", "Information" },
            { "Microsoft", "Warning" },
            { "Microsoft.Hosting", "Information" },
            { "Microsoft.EntityFrameworkCore.Database.Command", "Information" },
            { "Microsoft.EntityFrameworkCore.Query", "Information" },
            { "Microsoft.EntityFrameworkCore.Update", "Information" },
        };

        if (dict.ContainsKey("Default"))
        {
            builder = builder.WithEnvironment($"Serilog__MinimumLevel__Default", dict["Default"]);
        }
        foreach (var item in dict.Keys.Where(k => k != "Default"))
        {
           builder = builder.WithEnvironment($"Serilog__MinimumLevel__Override__{item}", dict[item]);
        }
        return builder;
    }
}
