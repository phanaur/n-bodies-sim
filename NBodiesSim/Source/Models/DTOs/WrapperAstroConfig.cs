/*
 * Wrapper para el parseado de astrosConfig.json
 */
namespace NBodiesSim.Source.Models.DTOs;

internal class WrapperAstroConfig
{
    public required List<AstroConfig> AstrosConfig { get; set; }
}