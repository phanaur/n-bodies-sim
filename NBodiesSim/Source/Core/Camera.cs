/*
 * This is the simulation camera engine. It updates the positions of the Solar System bodies
 * depending on which body is selected. It also handles smooth transitions when changing
 * the chosen body.
 */
using System.Numerics;
using Raylib_cs;

namespace NBodiesSim.Source.Core;

public class Camera
{
    // Current state properties
    public Vector2D Position { get; private set; } = Vector2D.Zero;
    public double DistanceScale { get; private set; } = 1e9; // Scale parameter for orbital distances
    public double RadiusScale { get; private set; } = 1e6; // Scale parameter for body radii
    
    // Viewport dimensions
    public int Width { get; set; }
    public int Height { get; set; }

    // "Target" properties for interpolation: set changes in scales for a smooth transition
    // as well as changes in the chosen body.
    public double TargetDistanceScale { get; set; } = 1e9;
    public double TargetRadiusScale { get; set; } = 1e6;
    public float TargetId { get; set; } = 0; // ID of the body to follow

    // Interpolation configuration
    private double LerpSpeed { get; set; } = 0.08;
    private const double InitialLerpSpeed = 0.08;

    public Camera()
    {
        Width = Raylib.GetScreenWidth();
        Height = Raylib.GetScreenHeight();
    }

    public void Update(double dt, Vector2D targetPosition)
    {
        // Update dimensions if the window changes
        Width = Raylib.GetScreenWidth();
        Height = Raylib.GetScreenHeight();

        // Visual interpolation (smooth, based on dt to be FPS-independent)
        double smoothFactor = 1.0 - Math.Pow(1.0 - LerpSpeed, dt * 60.0);

        RadiusScale += (TargetRadiusScale - RadiusScale) * smoothFactor;
        DistanceScale += (TargetDistanceScale - DistanceScale) * smoothFactor;
        
        // Move the camera toward the target position
        Position += (targetPosition - Position) * smoothFactor;

        // Update LerpSpeed gradually (acceleration effect in the transition)
        if (LerpSpeed < 1)
        {
            LerpSpeed = Math.Min(1.0, LerpSpeed * (1.0 + dt));
        }
    }

    public Vector2 Center => new Vector2((float)Width / 2, (float)Height / 2); // Returns the center of the screen
    public void ResetLerp()
    {
        LerpSpeed = InitialLerpSpeed; // Reset the variable when new transitions happen
    }
    
    // Helper method to convert world coordinates to the screen
    public Vector2 WorldToScreen(Vector2D worldPos)
    {
        Vector2 p = (worldPos - Position).ToVector2((float)DistanceScale);
        p.X += (float)Width / 2;
        p.Y += (float)Height / 2;
        return p;
    }
}
