/*
 * Esta clase contiene la información para la generación de los objetos de tipo AstroData, con todos los parámetros. A
 * partir de los objetos generados con esta clase se construyen los objetos Astro, que se utilizarán en la simulación.
 */
namespace NBodiesSim.Source.Models.DTOs;

internal abstract class AstroData
{
    public required float Id { get; set; }

    public float? ParentId { get; set; }

    public required string Name { get; set; }

    public required double Mass { get; set; }

    public required double Radius { get; set; }

    public required int[] Color { get; set; } // [r, g, b, a]

    public required double[] Position { get; set; } // [x, y]

    public required double[] Velocity { get; set; } // [x, y]

    public double DesiredTrailTime { get; set; } = 86400 * 30; // Tiempo en segundos (default: 30 días)

    // En caso de tener anillos (hecho entero por la IA):
    public bool HasRings { get; set; } = false;

    public double InnerRingRadius { get; set; } // en metros

    public double OuterRingRadius { get; set; } // en metros

    public int[]? RingColor { get; set; } // [r, g, b, a]
}