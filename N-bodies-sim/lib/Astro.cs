using System.ComponentModel.DataAnnotations;
using Raylib_cs;

namespace N_bodies_sim.lib;

public class Astro
{
    // Fixed variables
    public required string Name { get; init; }
    public required float Id { get; init; }

    public float? ParentId { get; init; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "Mass must be greater than 0")]
    public required double Mass { get; init; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "Radius must be greater than 0")]
    public required double Radius { get; init; }

    // Color
    public required Color Color { get; init; }

    // En caso de tener anillos (hecho entero por la IA)

    public bool HasRings { get; init; }
    public double InnerRingRadius { get; init; }  // en metros
    public double OuterRingRadius { get; init; }  // en metros
    public Color? RingColor { get; init; }  // [r, g, b, a]

    // Traza de la órbita
    public required Queue<Vector2D> Trail { get; init; } = new Queue<Vector2D>();
    public int? TrailLength { get; init; } = 100;

    // Non-fixed variables
    public required Vector2D Position { get; set; }
    public required Vector2D Velocity { get; set; }
    public required Vector2D Acceleration { get; set; }

}
