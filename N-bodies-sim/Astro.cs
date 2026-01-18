using System.ComponentModel.DataAnnotations;
using Raylib_cs;
using System.Numerics;
namespace N_bodies_sim;

public class Astro
{
    // Fixed variables
    public string? Name { get; set; }
    public float Id { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "Mass must be greater than 0")]
    public double Mass { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "Radius must be greater than 0")]
    public double Radius { get; set; }

    // Color
    public Color Color { get; set; }

    // Traza de la órbita
    public Queue<Vector2D> Trail { get; set; } = new Queue<Vector2D>();
    public int TrailLength { get; set; } = 100;

    // Non-fixed variables
    public Vector2D Position { get; set; }
    public Vector2D Velocity { get; set; }
    public Vector2D Acceleration { get; set; }

}
