/*
 * This class handles the simulation's physics calculations using the RUnge-Kutta 4 algorithm
 *
 * It consists of three four main calculation methods: CalculateK1, CalculateK2, CalculateK3 and CalculateK4. Each one
 * of them calculates the hypothetical velocity and acceleration of the bodies for fractions of a given time, dt:
 * 
 * - CalculateK1: Updates the astros acceleration parameters and returns the current values of velocity and position in t = 0
 * - CalculateK2: Calculates the acceleration and velocity of the bodies for t = dt / 2, using the results of K1
 * - CalculateK3: Calculates the acceleration and velocity of the bodies for t = dt / 2, using the results of K2
 * - CalculateK4: Calculates the acceleration and velocity of the bodies for t = dt, using the results of K3
 *
 * WARNING: Vector2D arrays hypotheticalPos and HypotheticalVel are used in CalculateK2, CalculateK3 and CalculateK4.
 *          Before this iteration of the RK4 implementation, those variables were created inside each method, separately.
 *          To reduce the CPU time in case of calculating thousands of body-to-body interactions, the variables were
 *          created in UpdateRk4 and passed to the different methods.
 *
 *          The first iteration of RK4 that included this feature didn't take into account what pass by reference and
 *          pass by value was, so every method returned hypotheticalVel after modifying the array. That implied that
 *          velK2, velK3 and velK4 were references to hypotheticalVel and so, they had the same value when updating the
 *          position and velocity of each body.
 *
 *          Now, each method returns hypotheticalVel.ToArray() to create a copi of that array status into a new variable,
 *          solving the problem. In the case of hypotheticalPos, there was no problem. Each index of the array
 *          was updated with the astro[i].Position + velKN * (dt / 2) --or just dt, in the case of CalculateK4--, so no
 *          error "reference problem" happened there.
 *
 * Subsequently, there is a method accessed by the simulation that encompasses the entire physics calculation. For that,
 * it calculates a weighted average of the results of K1, K2, K3 and K4 to update the position and velocity of the bodies.
 */
using NBodiesSim.Source.Core;
using NBodiesSim.Source.Models;

namespace NBodiesSim.Source.Systems;


public class PhysicsEngineRk4
{

    private static Vector2D[] CalcAccelerations(List<Astro> astros, Vector2D[] hypothPos)
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
                Vector2D rij = hypothPos[j] - hypothPos[i];
                Vector2D acji = PhysicsConstants.G * (1 / rij.LengthSquared()) * Vector2D.Normalize(rij); // Does not include the other planet's mass
                acc[i] += acji * astros[j].Mass;
                acc[j] -= acji * astros[i].Mass;
            }
        }
        return acc;
    }
    private static (Vector2D[] acck1, Vector2D[] velk1) CalculateK1(List<Astro> astros, Vector2D[] initialPos)
    {
        Vector2D[] acck1 = CalcAccelerations(astros, initialPos);
        Vector2D[] velk1 = astros.Select(a => a.Velocity).ToArray();

        return (acck1, velk1);
    }
    private static (Vector2D[] accK2, Vector2D[] velK2) CalculateK2(
        List<Astro> astros,
        double dt,
        Vector2D[] velK1,  // position slopes of k1
        Vector2D[] accK1,  // velocity slopes of k1
        Vector2D[] hypotheticalPos,
        Vector2D[] hypotheticalVel)
    {
        // 1. Calculate hypothetical positions and velocities

        for (int i = 0; i < astros.Count; i++)
        {
            hypotheticalPos[i] = astros[i].Position + velK1[i] * (dt / 2);
            hypotheticalVel[i] = astros[i].Velocity + accK1[i] * (dt / 2);
        }

        // 2. Calculate accelerations with those hypothetical positions
        Vector2D[] accK2 = CalcAccelerations(astros, hypotheticalPos);

        // 3. The slopes of k2 are calculated: accelerations and hypothetical velocities
        // Return a copy to avoid array aliasing
        return (accK2, hypotheticalVel.ToArray());
    }
    private static (Vector2D[] accK3, Vector2D[] velK3) CalculateK3(
        List<Astro> astros,
        double dt,
        Vector2D[] velK2,  // position slopes of k2
        Vector2D[] accK2,  // velocity slopes of k2
        Vector2D[] hypotheticalPos,
        Vector2D[] hypotheticalVel)
    {
        // 1. Calculate hypothetical positions and velocities

        for (int i = 0; i < astros.Count; i++)
        {
            hypotheticalPos[i] = astros[i].Position + velK2[i] * (dt / 2);
            hypotheticalVel[i] = astros[i].Velocity + accK2[i] * (dt / 2);
        }

        // 2. Calculate accelerations with those hypothetical positions
        Vector2D[] accK3 = CalcAccelerations(astros, hypotheticalPos);

        // 3. The slopes of k3 are calculated: accelerations and hypothetical velocities
        // Return a copy to avoid array aliasing
        return (accK3, hypotheticalVel.ToArray());
    }

    private static (Vector2D[] accK4, Vector2D[] velK4) CalculateK4(
        List<Astro> astros,
        double dt,
        Vector2D[] velK3,  // position slopes of k3
        Vector2D[] accK3,  // velocity slopes of k3
        Vector2D[] hypotheticalPos,
        Vector2D[] hypotheticalVel)
    {

        // 1. Calculate hypothetical positions and velocities

        for (int i = 0; i < astros.Count; i++)
        {
            hypotheticalPos[i] = astros[i].Position + velK3[i] * dt;
            hypotheticalVel[i] = astros[i].Velocity + accK3[i] * dt;
        }

        // 2. Calculate accelerations with those hypothetical positions
        Vector2D[] accK4 = CalcAccelerations(astros, hypotheticalPos);

        // 3. The slopes of k4 are calculated: accelerations and hypothetical velocities
        // Return a copy to avoid array aliasing
        return (accK4, hypotheticalVel.ToArray());
    }

    public void UpdateRk4(List<Astro> astros, double dt)
    {
        Vector2D[] initialPos = astros.Select(a => a.Position).ToArray();

        Vector2D[] hypotheticalPos = new Vector2D[astros.Count];
        Vector2D[] hypotheticalVel = new Vector2D[astros.Count];

        (Vector2D[] accK1, Vector2D[] velK1) = CalculateK1(astros, initialPos);

        (Vector2D[] accK2, Vector2D[] velK2) = CalculateK2(astros, dt, velK1, accK1, hypotheticalPos, hypotheticalVel);

        (Vector2D[] accK3, Vector2D[] velK3) = CalculateK3(astros, dt, velK2, accK2, hypotheticalPos, hypotheticalVel);

        (Vector2D[] accK4, Vector2D[] velK4) = CalculateK4(astros, dt, velK3, accK3, hypotheticalPos, hypotheticalVel);

        for (int i = 0; i < astros.Count; i++)
        {
            astros[i].Velocity += (accK1[i] + 2 * accK2[i] + 2 * accK3[i] + accK4[i]) * dt / 6;
            astros[i].Position += (velK1[i] + 2 * velK2[i] + 2 * velK3[i] + velK4[i]) * dt / 6;
        }
    }
}