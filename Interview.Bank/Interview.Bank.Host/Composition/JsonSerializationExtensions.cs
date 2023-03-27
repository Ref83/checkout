using System.Text.Json.Serialization;

namespace Interview.Bank.Host.Composition;

public static class JsonSerializationExtensions
{
    public static IMvcBuilder ConfigureJsonSerialization(this IMvcBuilder builder)
    {
        builder
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        return builder;
    }
    
}