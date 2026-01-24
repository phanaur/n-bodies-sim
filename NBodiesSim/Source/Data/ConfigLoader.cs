/*
 * This class loads the camera configuration from the Data/astrosConfig.json file located at the
 * project root.
 */
using System.Text.Json;
using NBodiesSim.Source.Models.DTOs;
using Raylib_cs;

namespace NBodiesSim.Source.Data;

public class ConfigLoader
{
    // Configuration dictionary. Each key has an associated AstroConfig object
    public Dictionary<KeyboardKey, AstroConfig> CameraConf { get; } = new Dictionary<KeyboardKey, AstroConfig>();

    // Parameters for deserializing the .json file
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true // Best practice
    };

    public ConfigLoader()
    {
        try
        {
            const string filePath = "Data/astrosConfig.json";

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            // Attempt to load from a JSON file
            string jsonAstroConfig = File.ReadAllText(filePath);

            WrapperAstroConfig? wrapperAstrosConfig =
                JsonSerializer.Deserialize<WrapperAstroConfig>(jsonAstroConfig, JsonOptions);

            // Now walk the deserialized list and build the objects
            if (wrapperAstrosConfig?.AstrosConfig == null)
            {
                throw new Exception("Error loading JSON file or it is empty");
            }

            // Iterate over each configuration from the JSON list
            foreach (AstroConfig config in wrapperAstrosConfig.AstrosConfig)
            {
                CameraConf[config.Key] = config;
            }
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine(e);
            throw;
        }
        catch (JsonException e)
        {
            Console.WriteLine(e);
            throw;
        }
        catch (IOException e)
        {
            Console.WriteLine(e);
            throw;
        }

    }
}
