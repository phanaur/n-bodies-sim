using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using Raylib_cs;
using System.Text.Json;
using N_bodies_sim.lib;
using N_bodies_sim.lib.wrapper;

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
        p.X += (float)_width / 2;
        p.Y += (float)_height / 2;
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
        const float margen = 30; // Distancia desde el borde
        Vector2 posTriangulo = center;

        // Calcular qué borde toca primero
        float distanciaX = float.MaxValue;
        float distanciaY = float.MaxValue;

        if (Math.Abs(direccion.X) > 0.001f)
        {
            float bordeX = direccion.X > 0 ? _width - margen : margen;
            distanciaX = Math.Abs((bordeX - center.X) / direccion.X);
        }

        if (Math.Abs(direccion.Y) > 0.001f)
        {
            float bordeY = direccion.Y > 0 ? _height - margen : margen;
            distanciaY = Math.Abs((bordeY - center.Y) / direccion.Y);
        }

        float distancia = Math.Min(distanciaX, distanciaY);
        posTriangulo.X = center.X + direccion.X * distancia;
        posTriangulo.Y = center.Y + direccion.Y * distancia;

        // 5. Dibujar el triángulo apuntando hacia el astro
        const float tamañoTriangulo = 10.0f;

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
        // Convertir Queue a array una sola vez para acceso O(1)
        Vector2D[] trailArray = astro.Trail.ToArray();
        int trailCount = trailArray.Length;

        if (trailCount < 2) return;

        // Pre-calcular límites de pantalla en coordenadas del mundo
        double worldWidth = _width * distanceScale;
        double worldHeight = _height * distanceScale;
        double margin = Math.Max(worldWidth, worldHeight) * 0.5; // Margen generoso para trails

        double minX = cameraPos.GetX() - worldWidth / 2 - margin;
        double maxX = cameraPos.GetX() + worldWidth / 2 + margin;
        double minY = cameraPos.GetY() - worldHeight / 2 - margin;
        double maxY = cameraPos.GetY() + worldHeight / 2 + margin;

        Vector2 p1 = Vector2.Zero;
        bool p1Valid = false;

        for (int i = 0; i < trailCount; i++)
        {
            Vector2D worldPos = trailArray[i];

            // Culling rápido en coordenadas del mundo
            double wx = worldPos.GetX();
            double wy = worldPos.GetY();
            bool inBounds = wx >= minX && wx <= maxX && wy >= minY && wy <= maxY;

            if (!inBounds && i < trailCount - 1)
            {
                // Verificar si el siguiente punto también está fuera
                Vector2D nextWorldPos = trailArray[i + 1];
                double nwx = nextWorldPos.GetX();
                double nwy = nextWorldPos.GetY();
                bool nextInBounds = nwx >= minX && nwx <= maxX && nwy >= minY && nwy <= maxY;

                if (!nextInBounds)
                {
                    p1Valid = false;
                    continue; // Ambos puntos fuera, skip
                }
            }

            if (!p1Valid)
            {
                p1 = Trail2Pixels(worldPos, distanceScale, cameraPos);
                p1Valid = true;
                continue;
            }

            Vector2 p2 = Trail2Pixels(worldPos, distanceScale, cameraPos);

            float alpha = (float)i / trailCount;
            Color colorTraza = new Color(astro.Color.R, astro.Color.G, astro.Color.B, (byte)(alpha * 200));

            Raylib.DrawLineV(startPos: p1, endPos: p2, color: colorTraza);

            p1 = p2;
        }
    }

    private static void DrawAstro(Astro astro, Vector2 posPantalla,
        double distanceScale, double radiusScale, Vector2D cameraPos, int textAlign)
    {
        float radio = (float)(astro.Radius / radiusScale);

        if (radio < 2.0f) radio = 2.0f;
        if (radio > 40f) radio = 40.0f;

        // Dibujamos la traza solo si hay dos posiciones o más
        if (astro.Trail.Count > 1) DrawTrail(astro, distanceScale, cameraPos);

        Raylib.DrawCircleV(center: posPantalla, radius: radio, color: astro.Color);
        // Opcional: Dibujar el nombre del astro cerca del triángulo
        Raylib.DrawText(astro.Name, (int)posPantalla.X - textAlign, (int)posPantalla.Y + textAlign, 12, astro.Color);
    }

    private static void DrawRings(Astro astroActual, double distanceScale, Vector2D cameraPos)
    {
        // posPantalla del astroActual
        Vector2 posPantalla = (astroActual.Position - cameraPos).ToVector2((float)distanceScale);
        posPantalla.X += (float)(_width) / 2;
        posPantalla.Y += (float)(_height) / 2;

        // Factor para que los anillos se vean proporcionalmente correctos
        double ringScale = distanceScale * 0.8;
        float innerRadius = (float)(astroActual.InnerRingRadius / ringScale);
        float outerRadius = (float)(astroActual.OuterRingRadius / ringScale);

        if (astroActual.RingColor != null)
            Raylib.DrawRing(posPantalla, innerRadius, outerRadius, 0, 360, 50, astroActual.RingColor.Value);
    }

    private static void DrawAsteroidBelt(Vector2 solPantalla, double distanceScale)
    {
        // Cinturón de asteroides (entre Marte y Júpiter)
        float asteroidBeltInner = (float)(3.3e11 / distanceScale);
        float asteroidBeltOuter = (float)(4.9e11 / distanceScale);
        // Solo dibujar si el cinturón puede ser visible en pantalla
        if (solPantalla.X + asteroidBeltOuter > 0 && solPantalla.X - asteroidBeltOuter < _width &&
            solPantalla.Y + asteroidBeltOuter > 0 && solPantalla.Y - asteroidBeltOuter < _height)
        {
            Raylib.DrawRing(solPantalla, asteroidBeltInner, asteroidBeltOuter, 0, 360, 100, new Color(139, 125, 107, 15));

            // Dibujar asteroides pequeños en el cinturón
            Random rnd = new Random(42); // Semilla fija para que siempre aparezcan en el mismo lugar
            const int numAsteroides = 50; // Número de asteroides a dibujar
            for (int i = 0; i < numAsteroides; i++)
            {
                double angulo = rnd.NextDouble() * 2 * Math.PI;
                double radioAsteroide = asteroidBeltInner + rnd.NextDouble() * (asteroidBeltOuter - asteroidBeltInner);

                float xAsteroide = solPantalla.X + (float)(Math.Cos(angulo) * radioAsteroide);
                float yAsteroide = solPantalla.Y + (float)(Math.Sin(angulo) * radioAsteroide);

                // Solo dibujar si está dentro de la pantalla
                if (xAsteroide >= 0 && xAsteroide <= _width && yAsteroide >= 0 && yAsteroide <= _height)
                {
                    float size = 1.0f + (float)rnd.NextDouble() * 1.5f;
                    Raylib.DrawCircleV(new Vector2(xAsteroide, yAsteroide), size, new Color(139, 125, 107, 180));
                }
            }

            // Texto: si es vista ampliada, fuera del cinturón; si no, entre el Sol y el anillo
            float textRadius = _keyName == "Espacio" ? asteroidBeltOuter * 1.15f : asteroidBeltInner * 0.7f;
            int textWidth = Raylib.MeasureText("Cinturón de asteroides", 12);
            Raylib.DrawText("Cinturón de asteroides", (int)(solPantalla.X - (float)(textWidth) / 2), (int)(solPantalla.Y - textRadius), 12, new Color(139, 125, 107, 150));
        }
    }

    private static void DrawKuiperBelt(Vector2 solPantalla, double distanceScale)
    {
        float kuiperBeltInner = (float)(5.2e12 / distanceScale);
        float kuiperBeltOuter = (float)(7.5e12 / distanceScale);
        // Solo dibujar si el cinturón puede ser visible en pantalla
        if (solPantalla.X + kuiperBeltOuter > 0 && solPantalla.X - kuiperBeltOuter < _width &&
            solPantalla.Y + kuiperBeltOuter > 0 && solPantalla.Y - kuiperBeltOuter < _height)
        {
            Raylib.DrawRing(solPantalla, kuiperBeltInner, kuiperBeltOuter, 0, 360, 100, new Color(100, 100, 120, 10));

            // Dibujar objetos pequeños en el cinturón de Kuiper
            Random rnd = new Random(123); // Semilla diferente para distribución diferente
            const int numObjetos = 200; // Más objetos porque es un cinturón más grande
            for (int i = 0; i < numObjetos; i++)
            {
                double angulo = rnd.NextDouble() * 2 * Math.PI;
                double radioObjeto = kuiperBeltInner + rnd.NextDouble() * (kuiperBeltOuter - kuiperBeltInner);

                float xObjeto = solPantalla.X + (float)(Math.Cos(angulo) * radioObjeto);
                float yObjeto = solPantalla.Y + (float)(Math.Sin(angulo) * radioObjeto);

                // Solo dibujar si está dentro de la pantalla
                if (!(xObjeto >= 0) || !(xObjeto <= _width) || !(yObjeto >= 0) || !(yObjeto <= _height)) continue;
                float size = 1.0f + (float)rnd.NextDouble() * 1.5f;
                Raylib.DrawCircleV(new Vector2(xObjeto, yObjeto), size, new Color(100, 100, 120, 150));
            }

            // Texto dentro del anillo, entre el Sol y el borde interior
            float textRadius = kuiperBeltInner * 0.7f; // 70% del radio interior
            int textWidth = Raylib.MeasureText("Cinturón de Kuiper", 12);
            Raylib.DrawText("Cinturón de Kuiper", (int)(solPantalla.X - (float)(textWidth) / 2), (int)(solPantalla.Y - textRadius), 12, new Color(100, 100, 120, 150));
        }
    }
    private static void DrawCross(Vector2 center, float ladoCruz, Vector2D cameraPos, Astro astroActual)
    {
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
                Color.DarkGray.R,
                Color.DarkGray.G,
                Color.DarkGray.B,
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

    private static void DrawStars(double distanceScale, Vector2D cameraPos)
    {
        // Calcular el área visible en coordenadas del mundo para culling temprano
        double worldWidth = _width * distanceScale;
        double worldHeight = _height * distanceScale;
        double margin = Math.Max(worldWidth, worldHeight) * 0.1; // 10% de margen

        double minX = cameraPos.GetX() - worldWidth / 2 - margin;
        double maxX = cameraPos.GetX() + worldWidth / 2 + margin;
        double minY = cameraPos.GetY() - worldHeight / 2 - margin;
        double maxY = cameraPos.GetY() + worldHeight / 2 + margin;

        // Dibujar fondo de estrellas transformando posiciones del mundo a pantalla
        foreach (var star in Stars)
        {
            // Culling en coordenadas del mundo (mucho más rápido que transformar primero)
            double starX = star.position.GetX();
            double starY = star.position.GetY();

            if (starX < minX || starX > maxX || starY < minY || starY > maxY)
            {
                continue;
            }

            // Transformar de coordenadas del mundo a coordenadas de pantalla
            Vector2 posPantalla = (star.position - cameraPos).ToVector2((float)distanceScale);
            posPantalla.X += (float)_width / 2;
            posPantalla.Y += (float)_height / 2;

            int brightness = (int)(star.brightness * 255);
            Raylib.DrawCircleV(posPantalla, star.size, new Color(brightness, brightness, brightness, 255));
        }
    }

    private static void Draw(List<Astro> astros, double distanceScale, double radiusScale, Vector2D cameraPos,
        Astro astroActual, float ladoCruz, int textAlign)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);

        DrawStars(distanceScale, cameraPos);

        Raylib.DrawText(
            text: "Simulador de N-cuerpos | Pulsa Escape (Esc) para salir ",
            posX: 10,
            posY: 10,
            fontSize: 20,
            color: Color.White
            );
        Raylib.DrawText(
            text: "Cámara: 0 - Sol | 1 - Mercurio | 2 - Venus | ··· | 8 - Neptuno | Espacio: Visión completa del sistema",
            posX: 10,
            posY: Raylib.GetRenderHeight() - 30,
            fontSize: 20,
            color: Color.White
            );
        Raylib.DrawText(
            text: $"FPS {Raylib.GetFPS()}", posX: _width -200, posY: 30, fontSize: 20, color: Color.Green);

        // Calculamos el vector correspondiente al centro de la pantalla

        Vector2 center = new Vector2((float)(_width) / 2, (float)(_height) / 2);

        // Calcular el radio del Sol en pantalla ANTES del bucle
        Astro sol = astros.First(a => a.Id == 0);
        float radioSol = (float)(sol.Radius / radiusScale);
        if (radioSol < 2.0f) radioSol = 2.0f;
        if (radioSol > 40f) radioSol = 40.0f;
        _sunRadiusAtScale = radioSol;

        // Calcular la posición del Sol en pantalla (en píxeles)
        Vector2 posSolPantalla = (sol.Position - cameraPos).ToVector2((float)distanceScale);
        posSolPantalla.X += (float)_width / 2;
        posSolPantalla.Y += (float)_height / 2;

        foreach (Astro astro in astros)
        {
            // Si es satélite y su planeta padre no está seleccionado, skip
            if (astro.ParentId.HasValue && (astro.ParentId.Value - astroActual.Id) != 0 )
            {
                continue;
            }

            Vector2 posPantalla = (astro.Position - cameraPos).ToVector2((float)distanceScale);
            posPantalla.X += (float)_width / 2;
            posPantalla.Y += (float)_height / 2;

            // Calcular la distancia en pantalla desde el Sol hasta el astro (en píxeles)
            float distanciaDesdeElSol = Vector2.Distance(posSolPantalla, posPantalla);

            // Si estamos en la vista amplia o viendo el Sol, y el astro está dentro del disco solar, ocultarlo
            if ((_keyName == "Espacio" || astroActual.Id == 0) && astro.Id != 0 && distanciaDesdeElSol < _sunRadiusAtScale)
            {
                continue;
            }

            // 1. Comprobamos si el astro está fuera de los límites de la pantalla
            bool fueraDePantalla = posPantalla.X < 0 || posPantalla.X > _width || posPantalla.Y < 0 || posPantalla.Y > _height;

            if (fueraDePantalla)
            {
                DrawTriangles(posPantalla, center, astro);
            }
            else
            {
                DrawAstro(astro, posPantalla, distanceScale, radiusScale, cameraPos, textAlign);
            }

        }

        // Dibujamos anillos si los tiene
        if (astroActual is { HasRings: true, RingColor: not null })
        {
            DrawRings(astroActual, distanceScale, cameraPos);
        }

        // Cinturones de asteroides (centrados en el Sol)

        // Cinturón de asteroides

        DrawAsteroidBelt(posSolPantalla, distanceScale);

        // Cinturón de Kuiper (más allá de Neptuno, desde 35 UA hasta 50 UA)

        DrawKuiperBelt(posSolPantalla, distanceScale);
        // Cruz Satelital

        DrawCross(center, ladoCruz, cameraPos, astroActual);
    }

    // Dimensiones de la Ventana
    private static int _width = Raylib.GetScreenWidth();
    private static int _height = Raylib.GetScreenHeight();
    private const double G = 6.674e-11;

    // Condición visibilidad Mercurio
    private static float _sunRadiusAtScale;
    private static string _keyName = "Null";

    // Fondo de estrellas (posiciones en el espacio, no en pantalla)
    private static readonly List<(Vector2D position, float brightness, float size)> Stars = new List<(Vector2D, float, float)>();

    private static void GenerateStars()
    {
        Stars.Clear();
        Random rndStars = new Random(12345); // Semilla fija para consistencia

        const int numStars = 10000; // Número fijo de estrellas

        for (int i = 0; i < numStars; i++)
        {
            // Generar ángulo aleatorio
            double angulo = rndStars.NextDouble() * 2 * Math.PI;

            // Distancia aleatoria muy lejana (más allá del cinturón de Kuiper: 1e13 a 5e13 metros)
            double distancia = rndStars.NextDouble() * 4e13;

            // Posición en coordenadas del mundo
            double x = Math.Cos(angulo) * distancia;
            double y = Math.Sin(angulo) * distancia;

            float brightness = (float)(0.3 + rndStars.NextDouble() * 0.7);
            float size = (float)(0.5 + rndStars.NextDouble() * 1.5);
            Stars.Add((new Vector2D(x, y), brightness, size));
        }
    }

    private static void Main()
    {


        // 4. Establecimiento de la configuración inicial

        double distanceScale = 1e9; // Cada pixel de la distancia entre astros representa 1000000 km
        double radiusScale = 1e6; // Cada pixel de un astro representa 2000 km.
        double timeStep = 86400; // Cada frame de la simulación significa un día terrestre en tiempo real.
        int n = 1000; // Número de cálculos en los que se dividirá timeStep. Cuanto mayor sea, mejor precisión.

        float id = 0; // Id del astro que la cámara seguirá inicialmente

        double targetDistanceScale = 1e9; // Objetivo de distanceScale
        double targetTimeStep = 86400; // Objetivo de timeStep
        double targetN = 1000; // Objetivo de n
        double targetRadiusScale = radiusScale; // Objetivo de radiusScale

        int textAlign = 40;
        const float ladoCruz = 10;


        const double initialLerpSpeed = 0.08;
        double lerpSpeed = initialLerpSpeed;

        // Cargando configuración de cámara y variables de configuración desde JSON

        // Intento de carga desde archivo JSON

        string jsonAstroConfig = File.ReadAllText("Data/astrosConfig.json");

        // Configuramos las opciones del deserializador para que acepte strings como enums
        var options = new JsonSerializerOptions
        {
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        // Le decimos qué tipo debe esperar
        WrapperAstroConfig? wrapperAstrosConfig = JsonSerializer.Deserialize<WrapperAstroConfig>(jsonAstroConfig, options);

        // Ahora recorremos toda la lista deserializada y construimos los objetos
        // Para ello generamos un foreach que recorra cada objeto de la lista wrapper

        if (wrapperAstrosConfig?.AstrosConfig == null)
        {
            throw new Exception("Error al cargar el archivo JSON o está vacío");
        }

        // Creamos el diccionario vacío que usaremos en el bucle principal
        // La clave será la tecla del teclado (KeyboardKey) y el valor será la configuración (AstroConfig)
        Dictionary<KeyboardKey, AstroConfig> cameraConf = new Dictionary<KeyboardKey, AstroConfig>();

        // Iteramos sobre cada configuración de la lista que viene del JSON
        // wrapperAstrosConfig.AstrosConfig es una List<AstroConfig>
        foreach (AstroConfig config in wrapperAstrosConfig.AstrosConfig)
        {
            // config es directamente un objeto AstroConfig de la lista
            // No necesitamos extraer kvp.Key ni kvp.Value porque ahora es una lista, no un diccionario

            // Añadimos al diccionario usando la tecla que viene dentro de config.Key
            // Ejemplo: si config.Key es KeyboardKey.One, entonces cameraConf[KeyboardKey.One] = config
            cameraConf[config.Key] = config;
        }

        Vector2D cameraPos = Vector2D.Zero;


        // 1. Crear la ventana en Raylib
        Raylib.SetConfigFlags(ConfigFlags.FullscreenMode | ConfigFlags.Msaa4xHint);
        Raylib.InitWindow(_width, _height, "Simulador de N-cuerpos");
        Raylib.SetTargetFPS(Raylib.GetMonitorRefreshRate(0));

        // Actualizar dimensiones después de crear la ventana
        _width = Raylib.GetScreenWidth();
        _height = Raylib.GetScreenHeight();

        // Generar estrellas iniciales (solo una vez)
        GenerateStars();


        // 2. Creación de la lista de astros. Los valores de cada astro serán introducidos en el Sistema Internacional
        // de Unidades. Se establecerá una relación entre el radio real de cada astro y cómo se verán en pantalla.

        List<Astro> astros = new List<Astro>();

        // Generación de astros

        // Intento de carga desde archivo JSON

        string jsonAstroData = File.ReadAllText("Data/astrosData.json");

        // Le decimos qué tipo debe esperar
        WrapperAstroData? wrapperAstroData = JsonSerializer.Deserialize<WrapperAstroData>(jsonAstroData);

        // Ahora recorremos toda la lista deserializada y construimos los objetos
        // Para ello generamos un foreach que recorra cada objeto de la lista wrapper

        if (wrapperAstroData?.Astros == null)
        {
            throw new Exception("Error al cargar el archivo JSON o está vacío");
        }


        foreach (AstroData data in wrapperAstroData.Astros)
        {
            // Hecho todo el foreach por la IA, entendiendo el código

            int trailLength = data.TrailLength;

            // Si es satélite, heredará el TrailLength de su planeta padre
            if (data.ParentId != null)
            {
                // Si ParentId existe, buscamos el astro que tenga el Id igual que el ParentId del satélite

                AstroData? parentData = wrapperAstroData.Astros.FirstOrDefault(p => (p.Id - data.ParentId) == 0);

                //Si parentData no está vacío, entonces asignamos el TrailLength del planeta padre a la variable

                if (parentData != null)
                {
                    trailLength = parentData.TrailLength;
                }
            }

            // Comprobamos si tiene anillos
            Color? ringColor = null;
            if (data is { HasRings: true, RingColor: not null })
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
                // Seguimos la misma lógica para la posición y la velocidad
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

        // 5. Bucle principal
        while (!Raylib.WindowShouldClose())
        {
            _width = Raylib.GetScreenWidth();
            _height = Raylib.GetScreenHeight();

            // Cálculos de la física de la simulación
            UpdatePhysics(astros, timeStep, n);

            // Save the trail position
            SaveTrail(astros);

            // Set Camera

            int key = Raylib.GetKeyPressed();

            if (key != (int)KeyboardKey.Null && cameraConf.ContainsKey((KeyboardKey)key))
            {
                var config = cameraConf[(KeyboardKey)key];
                id = config.Id;
                targetDistanceScale = config.TargetDistanceScale;
                targetRadiusScale = config.TargetRadiusScale;
                targetTimeStep = config.TargetTimeStep;
                targetN = config.TargetN;
                lerpSpeed = config.InitialLerpSpeed;
                textAlign = config.TextAlign;
                _keyName = config.KeyName;
            }

            Astro astroActual = astros.First(a => (a.Id - id) == 0);

            // Interpolación

            if (lerpSpeed > 100) lerpSpeed = 1; // Limitamos el lerpSpeed máximo

            // Escalado de radios por frame

            radiusScale += (targetRadiusScale - radiusScale) * lerpSpeed;

            // Escalado de distancias por frame

            distanceScale += (targetDistanceScale - distanceScale) * lerpSpeed;

            // Tiempo de simulación por frame

            timeStep += (targetTimeStep - timeStep) * lerpSpeed;

            // Número de cálculos n

            n += (int)((targetN - n) * lerpSpeed);


            // Cámara
            cameraPos += (astroActual.Position - cameraPos) * lerpSpeed;

            // Actualizar lerpspeed

            if (lerpSpeed < 1) lerpSpeed *= 1.01;

            // Dibujo del frame

            Draw(astros, distanceScale, radiusScale, cameraPos, astroActual, ladoCruz, textAlign);


        }

        Raylib.CloseWindow();
    }
}

