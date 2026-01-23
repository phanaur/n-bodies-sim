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
    public void UpdatePhysics(List<Astro> astros, double timestep)
    {
            UpdateAcceleration(astros);
            UpdateVelocity(astros, timestep);
            UpdatePosition(astros, timestep);
    }
}