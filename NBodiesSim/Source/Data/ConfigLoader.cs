using System.Text.Json;
using NBodiesSim.Source.Models.DTOs;
using Raylib_cs;

namespace NBodiesSim.Source.Data;

public class ConfigLoader
{
    public Dictionary<KeyboardKey, AstroConfig> CameraConf { get; } = new Dictionary<KeyboardKey, AstroConfig>();

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true // Buena práctica
    };

    public ConfigLoader()
    {
        // Intento de carga desde archivo JSON
        string jsonAstroConfig = File.ReadAllText("Data/astrosConfig.json");

        // Le decimos qué tipo debe esperar
        WrapperAstroConfig? wrapperAstrosConfig = JsonSerializer.Deserialize<WrapperAstroConfig>(jsonAstroConfig, JsonOptions);

        // Ahora recorremos toda la lista deserializada y construimos los objetos
        if (wrapperAstrosConfig?.AstrosConfig == null)
        {
            throw new Exception("Error al cargar el archivo JSON o está vacío");
        }

        // Iteramos sobre cada configuración de la lista que viene del JSON
        foreach (AstroConfig config in wrapperAstrosConfig.AstrosConfig)
        {
            CameraConf[config.Key] = config;
        }
    }
}