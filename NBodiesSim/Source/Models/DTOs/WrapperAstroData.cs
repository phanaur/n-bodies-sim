/*
 * Wrapper para el parseado de astrosData.json
 */
namespace NBodiesSim.Source.Models.DTOs;

internal class WrapperAstroData
{
    public required List<AstroData> Astros { get; set; }
}