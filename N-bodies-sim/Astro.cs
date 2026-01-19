using System.ComponentModel.DataAnnotations;
using Raylib_cs;
using System.Numerics;
namespace N_bodies_sim;

public class Astro
{
    // Fixed variables
    public required string Name { get; set; }
    public required float Id { get; set; }

    public float? ParentId { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "Mass must be greater than 0")]
    public required double Mass { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "Radius must be greater than 0")]
    public required double Radius { get; set; }

    // Color
    public required Color Color { get; set; }

    // En caso de tener anillos (hecho entero por la IA)

    public bool HasRings { get; set; } = false;
    public double InnerRingRadius { get; set; }  // en metros
    public double OuterRingRadius { get; set; }  // en metros
    public Color? RingColor { get; set; }  // [r, g, b, a]

    // Traza de la órbita
    public required Queue<Vector2D> Trail { get; set; } = new Queue<Vector2D>();
    public int? TrailLength { get; set; } = 100;

    // Non-fixed variables
    public required Vector2D Position { get; set; }
    public required Vector2D Velocity { get; set; }
    public required Vector2D Acceleration { get; set; }

}
