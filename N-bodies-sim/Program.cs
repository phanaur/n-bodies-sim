using System.Numerics;
using Raylib_cs;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace N_bodies_sim;

internal abstract class Program
{

    private static void UpdateAcceleration(List<Astro> astros)
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

    private static void UpdateVelocity(List<Astro> astros, double deltaTime)
    {
        foreach (Astro astro in astros)
        {
            astro.Velocity += astro.Acceleration * deltaTime;
        }
    }

    private static void UpdatePosition(List<Astro> astros, double deltaTime)
    {
        foreach (Astro astro in astros)
        {
            astro.Position += astro.Velocity * deltaTime;
        }
    }

    private static void UpdatePhysics(List<Astro> astros, double timeStep, double n)
    {
        for (int i = 0; i < n; i++)
        {
            UpdateAcceleration(astros);
            UpdateVelocity(astros, timeStep / n);
            UpdatePosition(astros, timeStep / n);
        }
    }

    private static void SaveTrail(List<Astro> astros)
    {
        foreach (Astro astro in astros)
        {
            // Guardamos la posición en la traza
            astro.Trail.Enqueue(astro.Position);

            if (astro.Trail.Count > astro.TrailLength) astro.Trail.Dequeue();
        }
    }

    private static Vector2 Trail2Pixels(Vector2D trailPoint, double distanceScale, Vector2D cameraPos)
    {
        // Convertimos posicion a pixeles
        Vector2 p = (trailPoint - cameraPos).ToVector2((float)distanceScale);

        // Centramos en pantalla
        p.X += (float)Width / 2;
        p.Y += (float)Height / 2;
        return p;
    }

    private static void Draw(List<Astro> astros, double distanceScale, double radiusScale, Vector2D cameraPos,
        Astro astroActual, float ladoCruz, double lerpSpeed)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);

        Raylib.DrawText(text: "Simulador de N-cuerpos",posX: 10, posY: 10, fontSize: 20, color: Color.White);
        Raylib.DrawText(
            text: "Cámara: 0 - Sol | 1 - Mercurio | 2 - Venus | ··· | 9 - Luna",
            posX: 10,
            posY: Height - 30,
            fontSize: 20,
            color: Color.White);
        Vector2 center = new Vector2((float)(Width) / 2, (float)(Height) / 2);

        foreach (Astro astro in astros)
        {
            Vector2 posPantalla = (astro.Position - cameraPos).ToVector2((float)distanceScale);
            posPantalla.X += (float)Width / 2;
            posPantalla.Y += (float)Height / 2;

            // 1. Comprobamos si el astro está fuera de los límites de la pantalla
            bool fueraDePantalla = posPantalla.X < 0 || posPantalla.X > Width || posPantalla.Y < 0 || posPantalla.Y > Height;

            if (fueraDePantalla)
            {
                // 1. Calcular dirección desde el centro de la pantalla hacia el astro
                Vector2 direccion = posPantalla - center;

                // 2. Normalizar la dirección
                float longitud = (float)Math.Sqrt(direccion.X * direccion.X + direccion.Y * direccion.Y);
                if (longitud > 0)
                {
                    direccion.X /= longitud;
                    direccion.Y /= longitud;
                }

                // 3. Calcular el ángulo de la dirección
                float angulo = (float)Math.Atan2(direccion.Y, direccion.X);

                // 4. Colocar el triángulo en el borde de la pantalla
                float margen = 30; // Distancia desde el borde
                Vector2 posTriangulo = center;

                // Calcular qué borde toca primero
                float distanciaX = float.MaxValue;
                float distanciaY = float.MaxValue;

                if (Math.Abs(direccion.X) > 0.001f)
                {
                    float bordeX = direccion.X > 0 ? Width - margen : margen;
                    distanciaX = Math.Abs((bordeX - center.X) / direccion.X);
                }

                if (Math.Abs(direccion.Y) > 0.001f)
                {
                    float bordeY = direccion.Y > 0 ? Height - margen : margen;
                    distanciaY = Math.Abs((bordeY - center.Y) / direccion.Y);
                }

                float distancia = Math.Min(distanciaX, distanciaY);
                posTriangulo.X = center.X + direccion.X * distancia;
                posTriangulo.Y = center.Y + direccion.Y * distancia;

                // 5. Dibujar el triángulo apuntando hacia el astro
                float tamañoTriangulo = 10.0f;

                // Vértice principal (punta) - apunta en la dirección del astro
                Vector2 v1 = posTriangulo + new Vector2(
                    (float)Math.Cos(angulo) * tamañoTriangulo,
                    (float)Math.Sin(angulo) * tamañoTriangulo
                );

                // Base del triángulo (dos vértices perpendiculares)
                float anguloBase1 = angulo + MathF.PI / 2; // 90 grados a la derecha
                float anguloBase2 = angulo - MathF.PI / 2; // 90 grados a la izquierda
                float anchoBase = tamañoTriangulo * 0.5f;

                Vector2 v2 = posTriangulo + new Vector2(
                    (float)Math.Cos(anguloBase1) * anchoBase,
                    (float)Math.Sin(anguloBase1) * anchoBase
                );

                Vector2 v3 = posTriangulo + new Vector2(
                    (float)Math.Cos(anguloBase2) * anchoBase,
                    (float)Math.Sin(anguloBase2) * anchoBase
                );

                // Debug: Dibujar un círculo donde debería estar el triángulo
                // Raylib.DrawCircleV(posTriangulo, 5, Color.Red);

                // Dibujar el triángulo (asegurándote de que esté en orden correcto)
                Raylib.DrawTriangle(v1, v3, v2, astro.Color);

                // Opcional: Dibujar el nombre del astro cerca del triángulo
                Raylib.DrawText(astro.Name, (int)posTriangulo.X - 15, (int)posTriangulo.Y + 15, 12, astro.Color);
            }
            else
            {
                // Dibujamos la traza solo si hay dos posiciones o más
                if (astro.Trail.Count > 1)
                {
                    for (int i = 0; i < astro.Trail.Count; i++)
                    {
                        // Dibujamos la línea
                        float alpha = (float)i / astro.Trail.Count;
                        Color colorTraza = new Color(astro.Color.R, astro.Color.G, astro.Color.B, (byte)(alpha * 200));
                        Vector2 p1 = Trail2Pixels(astro.Trail.ElementAt(i), distanceScale, cameraPos);

                        if (i < astro.Trail.Count - 1)
                        {
                            Vector2 p2 = Trail2Pixels(astro.Trail.ElementAt(i + 1), distanceScale, cameraPos);
                            Raylib.DrawLineV(
                                startPos: p1,
                                endPos: p2,
                                color: colorTraza);
                        }
                    }
                }

                float radio = (float)(astro.Radius / radiusScale);
                if (radio < 2.0f) radio = 2.0f;
                if (radio > 40f) radio = 40.0f;
                Raylib.DrawCircleV(center: posPantalla, radius: radio, color: astro.Color);
                // Opcional: Dibujar el nombre del astro cerca del triángulo
                Raylib.DrawText(astro.Name, (int)posPantalla.X - 15, (int)posPantalla.Y + 15, 12, astro.Color);
            }

        }
        // Cruz Satelital
        double distCamPlan = Vector2D.Distance(cameraPos, astroActual.Position);
        bool objBloqueado = distCamPlan < 1e7;

        Color colorCruz;

        if (objBloqueado)
        {
            // 1. Calculamos la oscilación suave (Seno da valores entre -1 y 1)
            // Bajamos a * 3 para que sea un "respirar" lento
            float oscilacion = (float)Math.Sin(Raylib.GetTime() * 3);

            // En la función Draw, cuando objBloqueado es true:
            float expansion = (float)Math.Sin(Raylib.GetTime() * 3) * 2; // Oscila +- 2 píxeles
            float ladoFinal = ladoCruz + expansion;

            // 2. Convertimos el rango [-1, 1] a [0.2, 1.0] para que nunca desaparezca del todo
            float factorOpacidad = (oscilacion * 0.4f) + 0.6f;

            // 3. Aplicamos la opacidad al color original del astro
            colorCruz = new Color(
                astroActual.Color.R,
                astroActual.Color.G,
                astroActual.Color.B,
                (byte)(factorOpacidad * 255)
            );
            Raylib.DrawLineV(center + new Vector2(ladoFinal, 0), center + new Vector2(-ladoFinal, 0), colorCruz);
            Raylib.DrawLineV(center + new Vector2(0, ladoFinal), center + new Vector2(0, -ladoFinal), colorCruz);

        }
        else
        {
            colorCruz = Color.DarkGray;
            Raylib.DrawLineV(center + new Vector2(ladoCruz, 0), center + new Vector2(-ladoCruz, 0), colorCruz);
            Raylib.DrawLineV(center + new Vector2(0, ladoCruz), center + new Vector2(0, -ladoCruz), colorCruz);
        }

        Raylib.EndDrawing();
    }

    // Dimensiones de la Ventana
    private const int Width = 1200;
    private const int Height = 800;
    private const double G = 6.674e-11;

    static void Main()
    {

        
        // 4. Establecimiento de escalas y tiempos
        double distanceScale = 1e9; // Cada pixel de la distancia entre astros representa 1000000 km
        const double radiusScale = 8e6; // Cada pixel de un astro representa 2000 km.
        double timeStep = 86400; // Cada frame de la simulación significa un día terrestre en tiempo real.
        int n = 1000; // Número de cálculos en los que se dividirá timeStep. Cuanto mayor sea, mejor precisión.
        
        // Id para la Cámara
        float id = 0;
        double targetDistanceScale = 1e9;
        double targetTimeStep = 86400;
        double targetN = 1000;
        const double initialLerpSpeed = 0.08;
        double lerpSpeed = initialLerpSpeed;
        Vector2D cameraPos = Vector2D.Zero;

    
        // 1. Crear la ventana en Raylib
        Raylib.InitWindow(Width, Height, "Simulador de N-cuerpos");
        Raylib.SetTargetFPS(60);

        // 2. Creación de la lista de astros. Los valores de cada astro serán introducidos en el Sistema Internacional
        // de Unidades. Se establecerá una relación entre el radio real de cada astro y cómo se verán en pantalla.

        List<Astro> astros = new List<Astro>();

        // Generación de astros

        // Sol
        Astro sol = new Astro
        {
            Id = 0,
            Name = "Sol",
            Mass = 1.989e30,
            Radius = 6.9634e8,
            Color = Color.Yellow,
            Position = Vector2D.Zero,
            Velocity = Vector2D.Zero,
            Acceleration = Vector2D.Zero
        };
        astros.Add(sol);

        // Mercurio
        Astro mercurio = new Astro
        {
            Id = 1,
            Name = "Mercurio",
            Mass = 3.285e23,
            Radius = 2.44e6,
            Position = new Vector2D(5.79e10, 0),
            Velocity = new Vector2D(0, 4.79e4),
            Acceleration = Vector2D.Zero,
            TrailLength = 365 + 100,
            Color = Color.Red
        };
        astros.Add(mercurio);

        // Venus
        Astro venus = new Astro
        {
            Id = 2,
            Name = "Venus",
            Mass = 4.867e24,
            Radius = 6.05e6,
            Position = new Vector2D(1.082e11, 0),
            Velocity = new Vector2D(0, 3.5e4),
            Acceleration = Vector2D.Zero,
            TrailLength = 365 + 100,
            Color = Color.Orange
        };
        astros.Add(venus);

        // Tierra
        Astro tierra = new Astro
        {
            Id = 3,
            Name = "Tierra",
            Mass = 5.972e24,
            Radius = 6.375e6,
            Position = new Vector2D(1.5e11, 0),
            Velocity = new Vector2D(0, 3e4),
            Acceleration = Vector2D.Zero,
            TrailLength = 365 + 100,
            Color = Color.Blue
        };
        astros.Add(tierra);


        // Luna
        Astro luna =new Astro
        {
            Id = 3.1f,
            Name = "Luna",
            Mass = 7.342e22,
            Radius = 1.737e6,
            Color = Color.Gray,
            Position =  tierra.Position + new Vector2D(3.844e8, 0),
            Velocity = tierra.Velocity + new Vector2D(0, 1.022e3),
            Acceleration = Vector2D.Zero,
            TrailLength = tierra.TrailLength //27 + 10
        };
        astros.Add(luna);

        // Marte
        Astro marte = new Astro
        {
            Id = 4,
            Name = "Marte",
            Mass = 6.39e23,
            Radius = 3.39e6,
            Position = new Vector2D(2.279e11, 0),
            Velocity = new Vector2D(0, 2.41e4),
            Acceleration = Vector2D.Zero,
            TrailLength = 365 + 100,
            Color = Color.Red
        };
        astros.Add(marte);

        // Júpiter
        Astro jupiter = new Astro
        {
            Id = 5,
            Name = "Júpiter",
            Mass = 1.898e27,
            Radius = 6.99e7,
            Position = new Vector2D(7.785e11, 0),
            Velocity = new Vector2D(0, 1.31e4),
            Acceleration = Vector2D.Zero,
            TrailLength = 365 + 100,
            Color = Color.Brown
        };
        astros.Add(jupiter);

        // Saturno
        Astro saturno = new Astro
        {
            Id = 6,
            Name = "Saturno",
            Mass = 5.683e26,
            Radius = 5.82e7,
            Position = new Vector2D(1.432e12, 0),
            Velocity = new Vector2D(0, 9.7e3),
            Acceleration = Vector2D.Zero,
            TrailLength = 365 + 100,
            Color = Color.Gold
        };
        astros.Add(saturno);
        
        // Urano
        Astro urano = new Astro
        {
            Id = 7,
            Name = "Urano",
            Mass = 8.681e25,
            Radius = 2.54e7,
            Position = new Vector2D(2.867e12, 0),
            Velocity = new Vector2D(0, 6.8e3),
            Acceleration = Vector2D.Zero,
            TrailLength = 365 + 100,
            Color = Color.SkyBlue
        };
        astros.Add(urano);
        
        // Neptuno
        Astro neptuno = new Astro
        {
            Id = 8,
            Name = "Neptuno",
            Mass = 1.024e26,
            Radius = 2.46e7,
            Position = new Vector2D(4.515e12, 0),
            Velocity = new Vector2D(0, 5.4e3),
            Acceleration = Vector2D.Zero,
            TrailLength = 365 + 100,
            Color = Color.Blue
        };
        astros.Add(neptuno);

        // Lado cruz objetivo
        float ladoCruz = (float)(astros.First(a => a.Id == id).Radius / radiusScale + 10) > 40
            ? (float)(astros.First(a => a.Id == id).Radius / radiusScale + 10)
            : 50;

        // 5. Bucle principal
        while (!Raylib.WindowShouldClose())
        {
            // Cálculos de la física de la simulación
            UpdatePhysics(astros, timeStep, n);

            // Save the trail position
            SaveTrail(astros);

            // Set Camera

            int key = Raylib.GetKeyPressed();

            if (key != (int)KeyboardKey.Null)
            {
                if (key != (int)KeyboardKey.Null)
                {
                    switch ((KeyboardKey)key)
                    {
                        case KeyboardKey.Zero:
                        case KeyboardKey.Kp0:
                            id = 0; // Sol
                            targetDistanceScale = 1e9; // Vista amplia del sistema solar interior
                            targetTimeStep = 86400; // 1 día
                            targetN = 1000;
                            lerpSpeed = initialLerpSpeed;
                            break;

                        case KeyboardKey.One:
                        case KeyboardKey.Kp1:
                            id = 1; // Mercurio
                            targetDistanceScale = 2e7; // Zoom cercano
                            targetTimeStep = 3600; // 1 hora
                            targetN = 5000;
                            lerpSpeed = initialLerpSpeed;
                            break;

                        case KeyboardKey.Two:
                        case KeyboardKey.Kp2:
                            id = 2; // Venus
                            targetDistanceScale = 2e7;
                            targetTimeStep = 3600;
                            targetN = 5000;
                            lerpSpeed = initialLerpSpeed;
                            break;

                        case KeyboardKey.Three:
                        case KeyboardKey.Kp3:
                            id = 3; // Tierra
                            targetDistanceScale = 2e7;
                            targetTimeStep = 3600;
                            targetN = 5000;
                            lerpSpeed = initialLerpSpeed;
                            break;

                        case KeyboardKey.Four:
                        case KeyboardKey.Kp4:
                            id = 4; // Marte
                            targetDistanceScale = 2e7;
                            targetTimeStep = 7200; // 2 horas
                            targetN = 5000;
                            lerpSpeed = initialLerpSpeed;
                            break;

                        case KeyboardKey.Five:
                        case KeyboardKey.Kp5:
                            id = 5; // Júpiter
                            targetDistanceScale = 2e7;
                            targetTimeStep = 86400; // 1 día (se mueve más lento)
                            targetN = 3000;
                            lerpSpeed = initialLerpSpeed;
                            break;

                        case KeyboardKey.Six:
                        case KeyboardKey.Kp6:
                            id = 6; // Saturno
                            targetDistanceScale = 2e7;
                            targetTimeStep = 86400 * 2; // 2 días
                            targetN = 2000;
                            lerpSpeed = initialLerpSpeed;
                            break;

                        case KeyboardKey.Seven:
                        case KeyboardKey.Kp7:
                            id = 7; // Urano
                            targetDistanceScale = 2e7;
                            targetTimeStep = 86400 * 7; // 1 semana
                            targetN = 1500;
                            lerpSpeed = initialLerpSpeed;
                            break;

                        case KeyboardKey.Eight:
                        case KeyboardKey.Kp8:
                            id = 8; // Neptuno
                            targetDistanceScale = 2e7;
                            targetTimeStep = 86400 * 7; // 1 semana
                            targetN = 1500;
                            lerpSpeed = initialLerpSpeed;
                            break;

                        case KeyboardKey.Nine:
                        case KeyboardKey.Kp9:
                            id = 3.1f; // Luna
                            targetDistanceScale = 2e7; // Zoom MUY cercano
                            targetTimeStep = 3600;
                            targetN = 10000;
                            lerpSpeed = initialLerpSpeed;
                            break;

                        default:
                            id = 0;
                            targetDistanceScale = 1e9;
                            targetTimeStep = 86400;
                            targetN = 1000;
                            lerpSpeed = initialLerpSpeed;
                            break;
                    }
                }
            }

            // Interpolación de la cámara
            if (lerpSpeed > 1) lerpSpeed = 1;

            distanceScale += (targetDistanceScale - distanceScale) * lerpSpeed;

            timeStep += (targetTimeStep - timeStep) * lerpSpeed;

            n += (int)((targetN - n) * lerpSpeed);

            Astro astroActual = astros.First(a => a.Id == id);

            cameraPos += (astroActual.Position - cameraPos) * lerpSpeed;

            if(lerpSpeed < 1) lerpSpeed *= 1.01;


            // Dibujo del frame
            ladoCruz += (float)(((astroActual.Radius / (2 * radiusScale)) + 10 - ladoCruz) * lerpSpeed);
            Draw(astros, distanceScale, radiusScale, cameraPos, astroActual, ladoCruz, lerpSpeed);


        }

        Raylib.CloseWindow();
    }
}

