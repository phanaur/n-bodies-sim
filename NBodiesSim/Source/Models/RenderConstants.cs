using Raylib_cs;

namespace NBodiesSim.Source.Models;

public static class RenderConstants
{
    // Triangle indicators for off-screen objects
    public const float Margin = 30f;
    public const float TriangleSize = 10.0f;

    // Celestial body size limits
    public const float MinBodyRadius = 2.0f;
    public const float MaxBodyRadius = 40.0f;

    // UI text
    public const int BodyNameFontSize = 12;

    // Asteroid and Kuiper belt
    public const int AsteroidNum = 50;
    public const int NumObjectsKuiper = 200;
    public const int BeltRingSegments = 100;

    // Cross animation
    public const float CrossPulseSpeed = 3f;
    public const float CrossExpansionAmplitude = 2f;

    // Belt colors
    public static readonly Color AsteroidBeltColor = new(139, 125, 107, 80);
    public static readonly Color AsteroidColor = new(139, 125, 107, 180);
    public static readonly Color KuiperBeltColor = new(100, 100, 120, 80);
    public static readonly Color KuiperObjectColor = new(100, 100, 120, 150);
}
