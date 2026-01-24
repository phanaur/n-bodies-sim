/*
 * Wrapper for parsing astrosConfig.json
 */
namespace NBodiesSim.Source.Models.DTOs;

internal class WrapperAstroConfig
{
    public required List<AstroConfig> AstrosConfig { get; init; }
}