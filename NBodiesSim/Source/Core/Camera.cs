/*
 * Este es el motor de cámara de la simulación. Se encarga de actualizar las posiciones de los cuerpos del Sistema Solar
 * en función de qué cuerpo se ha seleccionado. También se encarga de realizar la transición suave en los cambios del
 * cuerpo escogido.
 */
using System.Numerics;
using Raylib_cs;

namespace NBodiesSim.Source.Core;

public class Camera
{
    // Propiedades de estado actual
    public Vector2D Position { get; private set; } = Vector2D.Zero;
    public double DistanceScale { get; private set; } = 1e9; // Parámetro de escala para las distancias orbitales
    public double RadiusScale { get; private set; } = 1e6; // Parámetro de escala para los radios de los cuerpos
    
    // Dimensiones del viewport
    public int Width { get; set; }
    public int Height { get; set; }

    // Propiedades "objetivo" para interpolación: establece cambios en las escalas, para proceder a una transición suave
    // así como cambios en el cuerpo escogido.
    public double TargetDistanceScale { get; set; } = 1e9;
    public double TargetRadiusScale { get; set; } = 1e6;
    public float TargetId { get; set; } = 0; // ID del astro a seguir

    // Configuración de interpolación
    public double LerpSpeed { get; set; } = 0.08;
    private const double InitialLerpSpeed = 0.08;

    public Camera()
    {
        Width = Raylib.GetScreenWidth();
        Height = Raylib.GetScreenHeight();
    }

    public void Update(double dt, Vector2D targetPosition)
    {
        // Actualizar dimensiones si la ventana cambia
        Width = Raylib.GetScreenWidth();
        Height = Raylib.GetScreenHeight();

        // Interpolación visual (suave, basada en dt para ser independiente de FPS)
        double smoothFactor = 1.0 - Math.Pow(1.0 - LerpSpeed, dt * 60.0);

        RadiusScale += (TargetRadiusScale - RadiusScale) * smoothFactor;
        DistanceScale += (TargetDistanceScale - DistanceScale) * smoothFactor;
        
        // Mover la cámara hacia la posición del objetivo
        Position += (targetPosition - Position) * smoothFactor;

        // Actualizar LerpSpeed gradualmente (efecto de aceleración en la transición)
        if (LerpSpeed < 1)
        {
            LerpSpeed = Math.Min(1.0, LerpSpeed * (1.0 + dt));
        }
    }

    public Vector2 Center => new Vector2((float)Width / 2, (float)Height / 2);
    public void ResetLerp()
    {
        LerpSpeed = InitialLerpSpeed; // Reseteo de la variable cuando se realizan transiciones nuevas
    }
    
    // Método helper para convertir coordenadas del mundo a pantalla
    public Vector2 WorldToScreen(Vector2D worldPos)
    {
        Vector2 p = (worldPos - Position).ToVector2((float)DistanceScale);
        p.X += (float)Width / 2;
        p.Y += (float)Height / 2;
        return p;
    }
}