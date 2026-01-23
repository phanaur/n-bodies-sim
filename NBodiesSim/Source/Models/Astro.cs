/*
 * Esta es la clase Astro. Es el corazón de todos los cuerpos implicados en la simulación. Cada cuerpo simulado contine
 * información real acerca de algunas de sus características, como su nombre, identificación, radio del cuerpo, posición,
 * velocidad, etc.
 */
using System.ComponentModel.DataAnnotations;
using Raylib_cs;
using NBodiesSim.Source.Core;

namespace NBodiesSim.Source.Models;

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

    public double InnerRingRadius { get; init; } // en metros

    public double OuterRingRadius { get; init; } // en metros

    public Color? RingColor { get; init; } // [r, g, b, a]

    // Traza de la órbita
    public required Queue<Vector2D> Trail { get; init; } = new Queue<Vector2D>();

    public double DesiredTrailTime { get; init; } = 86400 * 30; // Tiempo en segundos que queremos ver (ej: 30 días)

    // Non-fixed variables
    public required Vector2D Position { get; set; }

    public required Vector2D PastPosition { get; set; }

    public required Vector2D RenderPosition { get; set; } = new Vector2D();

    public required Vector2D Velocity { get; set; }

    public required Vector2D Acceleration { get; set; }
}