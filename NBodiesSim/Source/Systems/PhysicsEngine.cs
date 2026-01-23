/*
 * Esta clase se encarga de los cálculos de la física de la simulación. De momento utiliza la integración numérica mediante
 * el método de Euler, pero se espera su actualización al uso del algoritmo Runge-Kuta 4.
 *
 * Consiste en tres métodos de cálculo: para la aceleración, para la velocidad y para la posición. Posteriormente existe
 * un método al que accede la simulación que engloba todo el cálculo de la física.
 */
using NBodiesSim.Source.Core;
using NBodiesSim.Source.Models;

namespace NBodiesSim.Source.Systems;


public class PhysicsEngine
{
    private const double G = 6.674e-11; // Constante de gravitación universal en m^3 kg^-1 s^-2

    private void UpdateAcceleration(List<Astro> astros)
    {
        // Reset acceleration
        foreach (Astro astro in astros)
        {
            astro.Acceleration = Vector2D.Zero;
        }

        // Cálculo de la aceleración para cada par de cuerpos. La aceleración que ejerce i sobre j tiene la misma dirección
        // y sentido opuesto que la que j have sobre i. Estas interacciones se acumulan en las respectivas aceleraciones.
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

    private Vector2D[] CalcAccelerations(List<Astro> astros, Vector2D[] posicionesHipotéticas)
    {
        // Cálculo de la aceleración para cada par de cuerpos. La aceleración que ejerce i sobre j tiene la misma dirección
        // y sentido opuesto que la que j have sobre i. Estas interacciones se acumulan en las respectivas aceleraciones.
        Vector2D[] acc = new Vector2D[astros.Count];
        for (int i = 0; i < astros.Count; i++)
        {
            acc[i] = Vector2D.Zero;
        }
        for (int i = 0; i < astros.Count - 1; i++)
        {
            for (int j = i + 1; j < astros.Count; j++)
            {
                Vector2D rij = posicionesHipotéticas[j] - posicionesHipotéticas[i];
                Vector2D acji = G * (1 / rij.LengthSquared()) * Vector2D.Normalize(rij); // No incluye la masa del otro planeta
                acc[i] += acji * astros[j].Mass;
                acc[j] -= acji * astros[i].Mass;
            }
        }
        return acc;
    }
    private (Vector2D[] acck1, Vector2D[] velk1) Calculatek1(List<Astro> astros)
    {
        UpdateAcceleration(astros);

        return (
            acck1: astros.Select(a => a.Acceleration).ToArray(),
            velk1: astros.Select(a => a.Velocity).ToArray());
    }
    private (Vector2D[] accK2, Vector2D[] velK2) CalculateK2(
        List<Astro> astros,
        double dt,
        Vector2D[] velK1,  // pendientes de posición de k1
        Vector2D[] accK1)  // pendientes de velocidad de k1
    {
        // 1. Calcular posiciones y velocidades hipotéticas
        Vector2D[] posHipoteticas = new Vector2D[astros.Count];
        Vector2D[] velHipoteticas = new Vector2D[astros.Count];

        for (int i = 0; i < astros.Count; i++)
        {
            posHipoteticas[i] = astros[i].Position + velK1[i] * (dt / 2);
            velHipoteticas[i] = astros[i].Velocity + accK1[i] * (dt / 2);
        }

        // 2. Calcular aceleraciones con esas posiciones hipotéticas
        Vector2D[] accK2 = CalcAccelerations(astros, posHipoteticas);

        // 3. Las pendientes de k2 son: aceleraciones calculadas y velocidades hipotéticas
        return (accK2, velHipoteticas);
    }
    private (Vector2D[] accK3, Vector2D[] velK3) CalculateK3(
        List<Astro> astros,
        double dt,
        Vector2D[] velK2,  // pendientes de posición de k1
        Vector2D[] accK2)  // pendientes de velocidad de k1
    {
        // 1. Calcular posiciones y velocidades hipotéticas
        Vector2D[] posHipoteticas = new Vector2D[astros.Count];
        Vector2D[] velHipoteticas = new Vector2D[astros.Count];

        for (int i = 0; i < astros.Count; i++)
        {
            posHipoteticas[i] = astros[i].Position + velK2[i] * (dt / 2);
            velHipoteticas[i] = astros[i].Velocity + accK2[i] * (dt / 2);
        }

        // 2. Calcular aceleraciones con esas posiciones hipotéticas
        Vector2D[] accK3 = CalcAccelerations(astros, posHipoteticas);

        // 3. Las pendientes de k2 son: aceleraciones calculadas y velocidades hipotéticas
        return (accK3, velHipoteticas);
    }

    private (Vector2D[] accK4, Vector2D[] velK4) CalculateK4(
        List<Astro> astros,
        double dt,
        Vector2D[] velK3,  // pendientes de posición de k1
        Vector2D[] accK3)  // pendientes de velocidad de k1
    {
        // 1. Calcular posiciones y velocidades hipotéticas
        Vector2D[] posHipoteticas = new Vector2D[astros.Count];
        Vector2D[] velHipoteticas = new Vector2D[astros.Count];

        for (int i = 0; i < astros.Count; i++)
        {
            posHipoteticas[i] = astros[i].Position + velK3[i] * dt;
            velHipoteticas[i] = astros[i].Velocity + accK3[i] * dt;
        }

        // 2. Calcular aceleraciones con esas posiciones hipotéticas
        Vector2D[] accK4 = CalcAccelerations(astros, posHipoteticas);

        // 3. Las pendientes de k2 son: aceleraciones calculadas y velocidades hipotéticas
        return (accK4, velHipoteticas);
    }

    public void UpdateRK4(List<Astro> astros, double dt)
    {
        (Vector2D[] acck1, Vector2D[] velk1) k1 = Calculatek1(astros);
        (Vector2D[] acck2, Vector2D[] velk2) k2 = CalculateK2(astros, dt, k1.velk1, k1.acck1);
        (Vector2D[] acck3, Vector2D[] velk3) k3 = CalculateK3(astros, dt, k2.velk2, k2.acck2);
        (Vector2D[] acck4, Vector2D[] velk4) k4 = CalculateK4(astros, dt, k3.velk3, k3.acck3);

        for (int i = 0; i < astros.Count; i++)
        {
            astros[i].Velocity += (k1.acck1[i] + 2 * k2.acck2[i] + 2 * k3.acck3[i] + k4.acck4[i]) * dt / 6;
            astros[i].Position += (k1.velk1[i] + 2 * k2.velk2[i] + 2 * k3.velk3[i] + k4.velk4[i]) * dt / 6;
        }
    }

    public void UpdatePhysics(List<Astro> astros, double timestep)
    {
            UpdateAcceleration(astros);
            UpdateVelocity(astros, timestep);
            UpdatePosition(astros, timestep);
    }
}