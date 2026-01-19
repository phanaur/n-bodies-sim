using System.Numerics;
using Raylib_cs;
using System.Text.Json;

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

    private static void DrawTriangles(Vector2 posPantalla, Vector2 center, Astro astro)
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

    private static void DrawTrail(Astro astro, double distanceScale, Vector2D cameraPos)
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

    private static void Draw(List<Astro> astros, double distanceScale, double radiusScale, Vector2D cameraPos,
        Astro astroActual, float ladoCruz, double lerpSpeed)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);

        Raylib.DrawText(text: "Simulador de N-cuerpos | Espacio: Visión completa del sistema", posX: 10, posY: 10, fontSize: 20, color: Color.White);
        Raylib.DrawText(
            text: "Cámara: 0 - Sol | 1 - Mercurio | 2 - Venus | ··· | 8 - Neptuno",
            posX: 10,
            posY: Raylib.GetRenderHeight() - 30,
            fontSize: 20,
            color: Color.White);
        Vector2 center = new Vector2((float)(Width) / 2, (float)(Height) / 2);

        foreach (Astro astro in astros)
        {
            // Si es satélite y su planeta padre no está seleccionado, skip
            if (astro.ParentId != null && astro.ParentId != astroActual.Id)
            {
                continue;
            }

            Vector2 posPantalla = (astro.Position - cameraPos).ToVector2((float)distanceScale);
            posPantalla.X += (float)Width / 2;
            posPantalla.Y += (float)Height / 2;

            // 1. Comprobamos si el astro está fuera de los límites de la pantalla
            bool fueraDePantalla = posPantalla.X < 0 || posPantalla.X > Width || posPantalla.Y < 0 || posPantalla.Y > Height;

            if (fueraDePantalla)
            {
                DrawTriangles(posPantalla, center, astro);
            }
            else
            {
                // Dibujamos la traza solo si hay dos posiciones o más
                if (astro.Trail.Count > 1) DrawTrail(astro, distanceScale, cameraPos);

                float radio = (float)(astro.Radius / radiusScale);
                if (radio < 2.0f) radio = 2.0f;
                if (radio > 40f) radio = 40.0f;
                Raylib.DrawCircleV(center: posPantalla, radius: radio, color: astro.Color);
                // Opcional: Dibujar el nombre del astro cerca del triángulo
                Raylib.DrawText(astro.Name, (int)posPantalla.X - 15, (int)posPantalla.Y + 15, 12, astro.Color);

            }

        }

        // Dibujamos anillos si los tiene
        if (astroActual.HasRings && astroActual.RingColor.HasValue)
        {
            // posPantalla del astroActual
            Vector2 posPantalla = (astroActual.Position - cameraPos).ToVector2((float)distanceScale);
            posPantalla.X += Width / 2;
            posPantalla.Y += Height / 2;

            float innerRadius = (float)(astroActual.InnerRingRadius / distanceScale);
            float outerRadius = (float)(astroActual.OuterRingRadius / distanceScale);

            Raylib.DrawRing(posPantalla, innerRadius, outerRadius, 0, 360, 50, astroActual.RingColor.Value);
        }

        // Cinturones de asteroides (centrados en el Sol)
        Vector2D solPos = astros.First(a => a.Id == 0).Position;
        Vector2 solPantalla = (solPos - cameraPos).ToVector2((float)distanceScale);
        solPantalla.X += Width / 2;
        solPantalla.Y += Height / 2;

        // Cinturón de asteroides (entre Marte y Júpiter)
        float asteroidBeltInner = (float)(3.3e11 / distanceScale);
        float asteroidBeltOuter = (float)(4.9e11 / distanceScale);
        // Solo dibujar si el cinturón puede ser visible en pantalla
        if (solPantalla.X + asteroidBeltOuter > 0 && solPantalla.X - asteroidBeltOuter < Width &&
            solPantalla.Y + asteroidBeltOuter > 0 && solPantalla.Y - asteroidBeltOuter < Height)
        {
            Raylib.DrawRing(solPantalla, asteroidBeltInner, asteroidBeltOuter, 0, 360, 100, new Color(139, 125, 107, 15));
            float asteroidBeltMid = (asteroidBeltInner + asteroidBeltOuter) / 2;
            Raylib.DrawText("Cinturón de asteroides", (int)(solPantalla.X + asteroidBeltMid), (int)solPantalla.Y - 5, 12, new Color(139, 125, 107, 150));
        }

        // Cinturón de Kuiper (más allá de Neptuno, desde 35 UA hasta 50 UA)
        float kuiperBeltInner = (float)(5.2e12 / distanceScale);
        float kuiperBeltOuter = (float)(7.5e12 / distanceScale);
        // Solo dibujar si el cinturón puede ser visible en pantalla
        if (solPantalla.X + kuiperBeltOuter > 0 && solPantalla.X - kuiperBeltOuter < Width &&
            solPantalla.Y + kuiperBeltOuter > 0 && solPantalla.Y - kuiperBeltOuter < Height)
        {
            Raylib.DrawRing(solPantalla, kuiperBeltInner, kuiperBeltOuter, 0, 360, 100, new Color(100, 100, 120, 10));
            float kuiperBeltMid = (kuiperBeltInner + kuiperBeltOuter) / 2;
            Raylib.DrawText("Cinturón de Kuiper", (int)(solPantalla.X + kuiperBeltMid), (int)solPantalla.Y - 5, 12, new Color(100, 100, 120, 150));
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
    private static int Width = Raylib.GetScreenWidth();
    private static int Height = Raylib.GetScreenHeight();
    private const double G = 6.674e-11;

    static void Main()
    {


        // 4. Establecimiento de escalas y tiempos
        double distanceScale = 1e9; // Cada pixel de la distancia entre astros representa 1000000 km
        double radiusScale = 8e6; // Cada pixel de un astro representa 2000 km.
        double targetRadiusScale = radiusScale;
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
        Raylib.SetConfigFlags(ConfigFlags.FullscreenMode | ConfigFlags.Msaa4xHint);
        Raylib.InitWindow(Width, Height, "Simulador de N-cuerpos");
        Raylib.SetTargetFPS(Raylib.GetMonitorRefreshRate(0));
        // 2. Creación de la lista de astros. Los valores de cada astro serán introducidos en el Sistema Internacional
        // de Unidades. Se establecerá una relación entre el radio real de cada astro y cómo se verán en pantalla.

        List<Astro> astros = new List<Astro>();

        // Generación de astros

        // Intento de carga desde archivo JSON

        string json = File.ReadAllText("Data/astros.json");

        // Le decimos qué tipo debe esperar
        WrapperClass? wrapper = JsonSerializer.Deserialize<WrapperClass>(json);

        // Ahora recorremos toda la lista deserializada y construimos los objetos
        // Para ello generamos un foreach que recorra cada objeto de la lista wrapper

        if (wrapper?.Astros == null)
        {
            throw new Exception("Error al cargar el archivo JSON o está vacío");
        }


        foreach (AstroData data in wrapper.Astros)
        {
            // Hecho todo el foreach por la IA, entendiendo el código

            int trailLength = (int)data.TrailLength;

            // Si es satélite, heredará el TrailLength de su planeta padre
            if (data.ParentId != null)
            {
                // Si ParentId existe, buscamos el astro que tenga el Id igual que el ParentId del satélite

                AstroData? parentData = wrapper.Astros.FirstOrDefault(p => p.Id == data.ParentId);

                //Si parentData no está vacío, entonces asignamos el TrailLength del planeta padre a la variable

                if (parentData != null)
                {
                    trailLength = (int)parentData.TrailLength;
                }
            }

            // Comprobamos si tiene anillos
            Color? ringColor = null;
            if (data.HasRings && data.RingColor != null)
            {
                ringColor = new Color(data.RingColor[0], data.RingColor[1], data.RingColor[2], data.RingColor[3]);
            }

            var astro = new Astro
            {
                Id = data.Id,
                ParentId = data.ParentId,
                Name = data.Name,
                Mass = data.Mass,
                Radius = data.Radius,
                // Generamos un color porque json no entiende el tipo Color, así que hemos guardado
                // el color en un array que representa r, g, b, a
                Color = new Color(data.Color[0], data.Color[1], data.Color[2], data.Color[3]),
                // Seguimos la misma lógica para la posicióny la velocidad
                Position = new Vector2D(data.Position[0], data.Position[1]),
                Velocity = new Vector2D(data.Velocity[0], data.Velocity[1]),
                Acceleration = Vector2D.Zero, // Siempre empieza con aceleración cero
                TrailLength = trailLength,
                Trail = new Queue<Vector2D>(), // Siempre la traza empieza vacía
                HasRings = data.HasRings,
                InnerRingRadius = data.InnerRingRadius,
                OuterRingRadius = data.OuterRingRadius,
                RingColor = ringColor

            };
            astros.Add(astro);
        }

        // Lado cruz objetivo
        float ladoCruz = (float)(astros.First(a => a.Id == id).Radius / radiusScale + 10) > 40
            ? (float)(astros.First(a => a.Id == id).Radius / radiusScale + 10)
            : 50;

        // 5. Bucle principal
        while (!Raylib.WindowShouldClose())
        {
            Width = Raylib.GetScreenWidth();
            Height = Raylib.GetScreenHeight();

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
                        case KeyboardKey.Space:
                            id = 0; // Sol
                            targetRadiusScale = 8e8;
                            targetDistanceScale = 1e10; // Vista muy amplia del sistema solar
                            targetTimeStep = 86400; // 1 día
                            targetN = 1000;
                            lerpSpeed = initialLerpSpeed;
                            break;
                        case KeyboardKey.Zero:
                        case KeyboardKey.Kp0:
                            id = 0; // Sol
                            targetRadiusScale = 8e7;
                            targetDistanceScale = 1e9; // Vista amplia del sistema solar interior
                            targetTimeStep = 86400; // 1 día
                            targetN = 1000;
                            lerpSpeed = initialLerpSpeed;
                            break;

                        case KeyboardKey.One:
                        case KeyboardKey.Kp1:
                            id = 1; // Mercurio
                            targetRadiusScale = 8e6;
                            targetDistanceScale = 2e7; // Zoom cercano
                            targetTimeStep = 1800; // 1/2 hora
                            targetN = 2000;
                            lerpSpeed = initialLerpSpeed;
                            break;

                        case KeyboardKey.Two:
                        case KeyboardKey.Kp2:
                            id = 2; // Venus
                            targetRadiusScale = 8e6;
                            targetDistanceScale = 2e7; // Zoom cercano
                            targetTimeStep = 1800;
                            targetN = 2000;
                            lerpSpeed = initialLerpSpeed;
                            break;

                        case KeyboardKey.Three:
                        case KeyboardKey.Kp3:
                            id = 3; // Tierra
                            targetRadiusScale = 8e5;
                            targetDistanceScale = 2e6;
                            targetTimeStep = 3600;
                            targetN = 2000;
                            lerpSpeed = initialLerpSpeed;
                            break;

                        case KeyboardKey.Four:
                        case KeyboardKey.Kp4:
                            id = 4; // Marte
                            targetRadiusScale = 8e5;
                            targetDistanceScale = 2e5;
                            targetTimeStep = 1800; // 1/2 hora
                            targetN = 2000;
                            lerpSpeed = initialLerpSpeed;
                            break;

                        case KeyboardKey.Five:
                        case KeyboardKey.Kp5:
                            id = 5; // Júpiter
                            targetRadiusScale = 8e6;
                            targetDistanceScale = 7e6;
                            targetTimeStep = 1800; // 1 día (se mueve más lento)
                            targetN = 1000;
                            lerpSpeed = initialLerpSpeed;
                            break;

                        case KeyboardKey.Six:
                        case KeyboardKey.Kp6:
                            id = 6; // Saturno
                            targetRadiusScale = 8e6;
                            targetDistanceScale = 5e6;
                            targetTimeStep = 3600; // 1 hora
                            targetN = 1000;
                            lerpSpeed = initialLerpSpeed;
                            break;

                        case KeyboardKey.Seven:
                        case KeyboardKey.Kp7:
                            id = 7; // Urano
                            targetRadiusScale = 3e6;
                            targetDistanceScale = 2e6;
                            targetTimeStep = 3600; // 1/2 día
                            targetN = 1500;
                            lerpSpeed = initialLerpSpeed;
                            break;

                        case KeyboardKey.Eight:
                        case KeyboardKey.Kp8:
                            id = 8; // Neptuno
                            targetRadiusScale = 3e6;
                            targetDistanceScale = 2e6;
                            targetTimeStep = 3600; // 1/2 día
                            targetN = 1500;
                            lerpSpeed = initialLerpSpeed;
                            break;

                        default:
                            id = 0;
                            targetRadiusScale = 8e6;
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

            radiusScale += (targetRadiusScale - radiusScale) * lerpSpeed;

            distanceScale += (targetDistanceScale - distanceScale) * lerpSpeed;

            timeStep += (targetTimeStep - timeStep) * lerpSpeed;

            n += (int)((targetN - n) * lerpSpeed);

            Astro astroActual = astros.First(a => a.Id == id);

            cameraPos += (astroActual.Position - cameraPos) * lerpSpeed;

            if (lerpSpeed < 1) lerpSpeed *= 1.01;

            // Actualizar targetLadoCruz basado en el radio del astro actual
            float targetLadoCruz = (float)(astroActual.Radius / (2 * radiusScale) + 10);

            // Dibujo del frame
            ladoCruz += (float)((targetLadoCruz - ladoCruz) * lerpSpeed);
            Draw(astros, distanceScale, radiusScale, cameraPos, astroActual, ladoCruz, lerpSpeed);


        }

        Raylib.CloseWindow();
    }
}

