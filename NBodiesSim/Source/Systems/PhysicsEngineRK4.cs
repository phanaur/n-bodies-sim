/*
 * This class handles the simulation's physics calculations using the RUnge-Kuta 4 algorithm
 *
 * It consists of three four main calculation methods: CalculateK1, CalculateK2, CalculateK3 and CalculateK4. Each one
 * of them calculates the hypothetical velocity and acceleration of the bodies for fractions of a given time, dt:
 * 
 * - CalculateK1: Updates the astros acceleration parameters and returns the current values of velocity and position in t = 0
 * - CalculateK2: Calculates the acceleration and velocity of the bodies for t = dt / 2, using the results of K1
 * - CalculateK3: Calculates the acceleration and velocity of the bodies for t = dt / 2, using the results of K2
 * - CalculateK4: Calculates the acceleration and velocity of the bodies for t = dt, using the results of K3
 * 
 * Subsequently, there is a method accessed by the simulation that encompasses the entire physics calculation. For that,
 * it calculates a weighted average of the results of K1, K2, K3 and K4 to update the position and velocity of the bodies.
 */
using NBodiesSim.Source.Core;
using NBodiesSim.Source.Models;

namespace NBodiesSim.Source.Systems;


public class PhysicsEngineRk4
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

    private Vector2D[] CalcAccelerations(List<Astro> astros, Vector2D[] hypotheticalPos)
    {
        // Calculation of acceleration for each pair of bodies. The acceleration exerted by i on j has the same direction
        // and opposite sense as the one j exerts on i. These interactions accumulate in the respective accelerations.
        Vector2D[] acc = new Vector2D[astros.Count];
        for (int i = 0; i < astros.Count; i++)
        {
            acc[i] = Vector2D.Zero;
        }
        for (int i = 0; i < astros.Count - 1; i++)
        {
            for (int j = i + 1; j < astros.Count; j++)
            {
                Vector2D rij = hypotheticalPos[j] - hypotheticalPos[i];
                Vector2D acji = G * (1 / rij.LengthSquared()) * Vector2D.Normalize(rij); // Does not include the other planet's mass
                acc[i] += acji * astros[j].Mass;
                acc[j] -= acji * astros[i].Mass;
            }
        }
        return acc;
    }
    private (Vector2D[] acck1, Vector2D[] velk1) CalculateK1(List<Astro> astros)
    {
        UpdateAcceleration(astros);

        return (
            acck1: astros.Select(a => a.Acceleration).ToArray(),
            velk1: astros.Select(a => a.Velocity).ToArray());
    }
    private (Vector2D[] accK2, Vector2D[] velK2) CalculateK2(
        List<Astro> astros,
        double dt,
        Vector2D[] velK1,  // position slopes of k1
        Vector2D[] accK1)  // velocity slopes of k1
    {
        // 1. Calculate hypothetical positions and velocities
        Vector2D[] hypotheticalPos = new Vector2D[astros.Count];
        Vector2D[] hypotheticalVel = new Vector2D[astros.Count];

        for (int i = 0; i < astros.Count; i++)
        {
            hypotheticalPos[i] = astros[i].Position + velK1[i] * (dt / 2);
            hypotheticalVel[i] = astros[i].Velocity + accK1[i] * (dt / 2);
        }

        // 2. Calculate accelerations with those hypothetical positions
        Vector2D[] accK2 = CalcAccelerations(astros, hypotheticalPos);

        // 3. The slopes of k2 are calculated: accelerations and hypothetical velocities
        return (accK2, hypotheticalVel);
    }
    private (Vector2D[] accK3, Vector2D[] velK3) CalculateK3(
        List<Astro> astros,
        double dt,
        Vector2D[] velK2,  // position slopes of k2
        Vector2D[] accK2)  // velocity slopes of k2
    {
        // 1. Calculate hypothetical positions and velocities
        Vector2D[] hypotheticalPos = new Vector2D[astros.Count];
        Vector2D[] hypotheticalVel = new Vector2D[astros.Count];

        for (int i = 0; i < astros.Count; i++)
        {
            hypotheticalPos[i] = astros[i].Position + velK2[i] * (dt / 2);
            hypotheticalVel[i] = astros[i].Velocity + accK2[i] * (dt / 2);
        }

        // 2. Calculate accelerations with those hypothetical positions
        Vector2D[] accK3 = CalcAccelerations(astros, hypotheticalPos);

        // 3. The slopes of k3 are calculated: accelerations and hypothetical velocities
        return (accK3, hypotheticalVel);
    }

    private (Vector2D[] accK4, Vector2D[] velK4) CalculateK4(
        List<Astro> astros,
        double dt,
        Vector2D[] velK3,  // position slopes of k3
        Vector2D[] accK3)  // velocity slopes of k3
    {
        // 1. Calculate hypothetical positions and velocities
        Vector2D[] posHipoteticas = new Vector2D[astros.Count];
        Vector2D[] velHipoteticas = new Vector2D[astros.Count];

        for (int i = 0; i < astros.Count; i++)
        {
            posHipoteticas[i] = astros[i].Position + velK3[i] * dt;
            velHipoteticas[i] = astros[i].Velocity + accK3[i] * dt;
        }

        // 2. Calculate accelerations with those hypothetical positions
        Vector2D[] accK4 = CalcAccelerations(astros, posHipoteticas);

        // 3. The slopes of k4 are calculated: accelerations and hypothetical velocities
        return (accK4, velHipoteticas);
    }

    public void UpdateRk4(List<Astro> astros, double dt)
    {
        (Vector2D[] acck1, Vector2D[] velk1) k1 = CalculateK1(astros);
        (Vector2D[] acck2, Vector2D[] velk2) k2 = CalculateK2(astros, dt, k1.velk1, k1.acck1);
        (Vector2D[] acck3, Vector2D[] velk3) k3 = CalculateK3(astros, dt, k2.velk2, k2.acck2);
        (Vector2D[] acck4, Vector2D[] velk4) k4 = CalculateK4(astros, dt, k3.velk3, k3.acck3);

        for (int i = 0; i < astros.Count; i++)
        {
            astros[i].Velocity += (k1.acck1[i] + 2 * k2.acck2[i] + 2 * k3.acck3[i] + k4.acck4[i]) * dt / 6;
            astros[i].Position += (k1.velk1[i] + 2 * k2.velk2[i] + 2 * k3.velk3[i] + k4.velk4[i]) * dt / 6;
        }
    }
}