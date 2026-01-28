/*
 * This class handles the simulation's physics calculations using the Euler method. It uses three main
 * calculation methods: for acceleration, for velocity, and for position.
 * Subsequently, there is a method accessed by the simulation that encompasses the entire physics calculation.
 *
 * Currently, this Engine is deprecated and has been replaced by the PhysicsEngineRK4 class.
 */
using NBodiesSim.Source.Core;
using NBodiesSim.Source.Models;

namespace NBodiesSim.Source.Systems;

[Obsolete("This class is deprecated and has been replaced by PhysicsEngineRK4")]
internal class PhysicsEngineEuler
{
    private const double G = 6.674e-11; // Universal gravitational constant in m^3 kg^-1 s^-2

    private void UpdateAcceleration(List<Astro> astros)
    {
        // Reset acceleration
        foreach (Astro astro in astros)
        {
            astro.Acceleration = Vector2D.Zero;
        }

        // Calculation of acceleration for each pair of bodies. The acceleration exerted by i on j has the same direction
        // and opposite sense as the one j exerts on i. These interactions accumulate in the respective accelerations.
        for (int i = 0; i < astros.Count - 1; i++)
        {
            for (int j = i + 1; j < astros.Count; j++)
            {
                Vector2D rij = astros[j].Position - astros[i].Position;
                Vector2D acji = G * (1 / rij.LengthSquared()) * Vector2D.Normalize(rij); // Does not include the other planet's mass
                astros[i].Acceleration += acji * astros[j].Mass;
                astros[j].Acceleration -= acji * astros[i].Mass;
            }
        }
    }

    private void UpdateVelocity(List<Astro> astros, double dt)
    {
        foreach (Astro astro in astros)
        {
            astro.Velocity += astro.Acceleration * dt;
        }
    }

    private void UpdatePosition(List<Astro> astros, double dt)
    {
        foreach (Astro astro in astros)
        {
            astro.Position += astro.Velocity * dt;
        }
    }

    public void UpdatePhysics(List<Astro> astros, double timestep)
    {
        UpdateAcceleration(astros);
        UpdateVelocity(astros, timestep);
        UpdatePosition(astros, timestep);
    }
}