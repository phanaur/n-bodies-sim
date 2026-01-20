using NBodiesSim.Source.Core;
using NBodiesSim.Source.Models;

namespace NBodiesSim.Source.Systems;


public class PhysicsEngine
{
    private const double G = 6.674e-11; // Constante de gravitación universal en m^3 kg^-1 s^-2

    private void UpdateAcceleration(List<Astro> astros)
    {
        foreach (Astro astro in astros)
        {
            astro.Acceleration = Vector2D.Zero;
        }

        for (int i = 0; i < astros.Count - 1; i++)
        {
            for (int j = i + 1; j < astros.Count; j++)
            {
                Vector2D rij = astros[j].Position - astros[i].Position;
                Vector2D acji = G * (1 / rij.LengthSquared()) * Vector2D.Normalize(rij); // No incluye la masa del otro planeta
                astros[i].Acceleration += acji * astros[j].Mass;
                astros[j].Acceleration -= acji * astros[i].Mass;
            }
        }
    }

    private void UpdateVelocity(List<Astro> astros, double deltaTime)
    {
        foreach (Astro astro in astros)
        {
            astro.Velocity += astro.Acceleration * deltaTime;
        }
    }

    private void UpdatePosition(List<Astro> astros, double deltaTime)
    {
        foreach (Astro astro in astros)
        {
            astro.Position += astro.Velocity * deltaTime;
        }
    }
    public void UpdatePhysics(List<Astro> astros, double timeStep, double n)
    {
        for (int i = 0; i < n; i++)
        {
            UpdateAcceleration(astros);
            UpdateVelocity(astros, timeStep / n);
            UpdatePosition(astros, timeStep / n);
        }
    }
}