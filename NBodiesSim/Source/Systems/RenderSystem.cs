/*
 * Esta clase contiene el motor de renderizado de la simulación. Calcula las conversiones de escala para las posiciones
 * de cada cuerpo, establece si el cuerpo debe aparecer en pantalla o no, dibuja el fondo de estrellas, los anillos del
 * planeta (si tiene). También establece una condición en función de los factores de escala distanceScale y radiusScale,
 * para que aquel planeta cuya órbita esté cerca del radio externo del Sol, o dentro, no se dibuje.
 */
using System.Numerics;
using NBodiesSim.Source.Core;
using NBodiesSim.Source.Models;
using Raylib_cs;

namespace NBodiesSim.Source.Systems;

public class RenderSystem
{
    // Obtención del radio del Sol en pantalla (en píxeles)
    private static float GetSunRadius(Astro sol, double radiusScale)
    {
        float radioSol = (float)(sol.Radius / radiusScale);

        // Establece condiciones para el tamaño máximo y mínimo del Sol.
        switch (radioSol)
        {
            case < 2.0f:
                return 2.0f;
            case > 40f:
                return 40.0f;
        }
        return radioSol;

    }

    // Obtención de la posición del Sol en pantalla (en píxeles)
    private static Vector2 GetSunPositionScreen(Astro sol, Camera camera)
    {
        return camera.WorldToScreen(sol.Position);
    }

    // Generación de las coordenadas para posicionar el triángulo que representa un planeta que no se vea en pantalla.
    private static (Vector2[], Vector2) GetTriangle(Vector2 posPantalla, Vector2 center, int width, int height)
    {
        Vector2[] triangle = new Vector2[3];
        // 1. Calcular dirección desde el centro de la pantalla hacia el astro
        Vector2 direccion = posPantalla - center;

        // 2. Normalizar la dirección
        float longitud = (float)Math.Sqrt((direccion.X * direccion.X) + (direccion.Y * direccion.Y));
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
            float bordeX = direccion.X > 0 ? width - margen : margen;
            distanciaX = Math.Abs((bordeX - center.X) / direccion.X);
        }

        if (Math.Abs(direccion.Y) > 0.001f)
        {
            float bordeY = direccion.Y > 0 ? height - margen : margen;
            distanciaY = Math.Abs((bordeY - center.Y) / direccion.Y);
        }

        float distancia = Math.Min(distanciaX, distanciaY);
        posTriangulo.X = center.X + (direccion.X * distancia);
        posTriangulo.Y = center.Y + (direccion.Y * distancia);

        // 5. Dibujar el triángulo apuntando hacia el astro
        const float tamañoTriangulo = 10.0f;

        // Vértice principal (punta) - apunta en la dirección del astro
        triangle[0] = posTriangulo + new Vector2(
            (float)Math.Cos(angulo) * tamañoTriangulo,
            (float)Math.Sin(angulo) * tamañoTriangulo);

        // Base del triángulo (dos vértices perpendiculares)
        float anguloBase1 = angulo + (MathF.PI / 2); // 90 grados a la derecha
        float anguloBase2 = angulo - (MathF.PI / 2); // 90 grados a la izquierda
        float anchoBase = tamañoTriangulo * 0.5f;

        triangle[1] = posTriangulo + new Vector2(
            (float)Math.Cos(anguloBase1) * anchoBase,
            (float)Math.Sin(anguloBase1) * anchoBase);

        triangle[2] = posTriangulo + new Vector2(
            (float)Math.Cos(anguloBase2) * anchoBase,
            (float)Math.Sin(anguloBase2) * anchoBase);

        return (triangle, posTriangulo);
    }
    public static void SaveTrail(List<Astro> astros)
    {
        foreach (Astro astro in astros)
        {
            // Guardamos la posición en la traza
            astro.Trail.Enqueue(astro.RenderPosition);

            if (astro.Trail.Count > astro.TrailLength)
            {
                astro.Trail.Dequeue();
            }
        }
    }

    private static void DrawTriangles(Vector2 posPantalla, Camera camera, Astro astro)
    {

        (Vector2[], Vector2) triangle = GetTriangle(posPantalla, camera.Center, camera.Width, camera.Height);
        // Debug: Dibujar un círculo donde debería estar el triángulo
        // Raylib.DrawCircleV(posTriangulo, 5, Color.Red);

        // Dibujar el triángulo (asegurándote de que esté en orden correcto)
        Raylib.DrawTriangle(triangle.Item1[0], triangle.Item1[2], triangle.Item1[1], astro.Color);

        // Opcional: Dibujar el nombre del astro cerca del triángulo
        Raylib.DrawText(astro.Name, (int)triangle.Item2.X - 15, (int)triangle.Item2.Y + 15, 12, astro.Color);
    }

    private static void DrawTrail(Astro astro, Camera camera)
    {
        if (astro.Trail.Count < 2)
        {
            return;
        }

        // Usamos Rlgl para dibujar todas las líneas en una sola llamada (Batching)
        // Esto es mucho más rápido que llamar a DrawLineV miles de veces
        Rlgl.Begin(DrawMode.Lines);
        Rlgl.SetTexture(0); // Usar textura blanca por defecto

        Vector2? p1 = null;
        int i = 0;
        int totalPoints = astro.Trail.Count;

        // Iteramos directamente sobre la Queue sin convertir a Array (evita asignación de memoria)
        foreach (var worldPos in astro.Trail)
        {
            Vector2 p2 = camera.WorldToScreen(worldPos);

            if (p1.HasValue)
            {
                // Calculamos el alpha basado en la posición en la cola
                float alpha = (float)i / totalPoints;
                byte alphaByte = (byte)(alpha * 200);

                // Asignamos color al vértice actual
                Rlgl.Color4ub(astro.Color.R, astro.Color.G, astro.Color.B, alphaByte);

                // Dibujamos el segmento p1 -> p2
                Rlgl.Vertex2f(p1.Value.X, p1.Value.Y);
                Rlgl.Vertex2f(p2.X, p2.Y);
            }

            p1 = p2;
            i++;
        }

        Rlgl.End();
    }

    private static void DrawAstro(
        Astro astro,
        Vector2 posPantalla,
        Camera camera,
        int textAlign)
    {
        float radio = (float)(astro.Radius / camera.RadiusScale);

        if (radio < 2.0f)
        {
            radio = 2.0f;
        }

        if (radio > 40f)
        {
            radio = 40.0f;
        }

        // Dibujamos la traza solo si hay dos posiciones o más
        if (astro.Trail.Count > 1) DrawTrail(astro, camera);

        Raylib.DrawCircleV(center: posPantalla, radius: radio, color: astro.Color);

        // Opcional: Dibujar el nombre del astro cerca del triángulo
        Raylib.DrawText(astro.Name, (int)posPantalla.X - textAlign, (int)posPantalla.Y + textAlign, 12, astro.Color);
    }

    private static void DrawRings(Astro astroActual, Camera camera)
    {
        if (astroActual is { HasRings: true, RingColor: not null })
        {
            // posPantalla del astroActual
            Vector2 posPantalla = camera.WorldToScreen(astroActual.Position);

            // Factor para que los anillos se vean proporcionalmente correctos
            double ringScale = camera.DistanceScale * 0.8;
            float innerRadius = (float)(astroActual.InnerRingRadius / ringScale);
            float outerRadius = (float)(astroActual.OuterRingRadius / ringScale);

            if (astroActual.RingColor != null)
            {
                Raylib.DrawRing(
                    center: posPantalla,
                    innerRadius,
                    outerRadius,
                    startAngle: 0,
                    endAngle: 360,
                    segments: 50,
                    astroActual.RingColor.Value);
            }
        }

    }

    private static void DrawAsteroidBelt(Vector2 solPantalla, Camera camera, string keyName)
    {
        // Cinturón de asteroides (entre Marte y Júpiter)
        float asteroidBeltInner = (float)(3.3e11 / camera.DistanceScale);
        float asteroidBeltOuter = (float)(4.9e11 / camera.DistanceScale);

        // Solo dibujar si el cinturón puede ser visible en pantalla
        if (!(solPantalla.X + asteroidBeltOuter > 0) || !(solPantalla.X - asteroidBeltOuter < camera.Width) ||
            !(solPantalla.Y + asteroidBeltOuter > 0) || !(solPantalla.Y - asteroidBeltOuter < camera.Height)) return;
        Raylib.DrawRing(solPantalla, asteroidBeltInner, asteroidBeltOuter, 0, 360, 100, new Color(139, 125, 107, 15));

        // Dibujar asteroides pequeños en el cinturón
        Random rnd = new Random(42); // Semilla fija para que siempre aparezcan en el mismo lugar
        const int numAsteroides = 50; // Número de asteroides a dibujar
        for (int i = 0; i < numAsteroides; i++)
        {
            double angulo = rnd.NextDouble() * 2 * Math.PI;
            double radioAsteroide = asteroidBeltInner + (rnd.NextDouble() * (asteroidBeltOuter - asteroidBeltInner));

            float xAsteroide = solPantalla.X + (float)(Math.Cos(angulo) * radioAsteroide);
            float yAsteroide = solPantalla.Y + (float)(Math.Sin(angulo) * radioAsteroide);

            // Solo dibujar si está dentro de la pantalla
            if (!(xAsteroide >= 0) || !(xAsteroide <= camera.Width) || !(yAsteroide >= 0) ||
                !(yAsteroide <= camera.Height)) continue;
            float size = 1.0f + ((float)rnd.NextDouble() * 1.5f);
            Raylib.DrawCircleV(new Vector2(xAsteroide, yAsteroide), size, new Color(139, 125, 107, 180));
        }

        // Texto: si es vista ampliada, fuera del cinturón; si no, entre el Sol y el anillo
        float textRadius = keyName == "Espacio" ? asteroidBeltOuter * 1.15f : asteroidBeltInner * 0.7f;
        int textWidth = Raylib.MeasureText("Cinturón de asteroides", 12);
        Raylib.DrawText("Cinturón de asteroides", (int)(solPantalla.X - ((float)textWidth / 2)), (int)(solPantalla.Y - textRadius), 12, new Color(139, 125, 107, 150));
    }

    private static void DrawKuiperBelt(Vector2 solPantalla, Camera camera)
    {
        float kuiperBeltInner = (float)(5.2e12 / camera.DistanceScale);
        float kuiperBeltOuter = (float)(7.5e12 / camera.DistanceScale);

        // Solo dibujar si el cinturón puede ser visible en pantalla
        if (!(solPantalla.X + kuiperBeltOuter > 0) || !(solPantalla.X - kuiperBeltOuter < camera.Width) ||
            !(solPantalla.Y + kuiperBeltOuter > 0) || !(solPantalla.Y - kuiperBeltOuter < camera.Height)) return;
        Raylib.DrawRing(solPantalla, kuiperBeltInner, kuiperBeltOuter, 0, 360, 100, new Color(100, 100, 120, 10));

        // Dibujar objetos pequeños en el cinturón de Kuiper
        Random rnd = new Random(123); // Semilla diferente para distribución diferente
        const int numObjetos = 200; // Más objetos porque es un cinturón más grande
        for (int i = 0; i < numObjetos; i++)
        {
            double angulo = rnd.NextDouble() * 2 * Math.PI;
            double radioObjeto = kuiperBeltInner + (rnd.NextDouble() * (kuiperBeltOuter - kuiperBeltInner));

            float xObjeto = solPantalla.X + (float)(Math.Cos(angulo) * radioObjeto);
            float yObjeto = solPantalla.Y + (float)(Math.Sin(angulo) * radioObjeto);

            // Solo dibujar si está dentro de la pantalla
            if (!(xObjeto >= 0) || !(xObjeto <= camera.Width) || !(yObjeto >= 0) || !(yObjeto <= camera.Height)) continue;

            float size = 1.0f + ((float)rnd.NextDouble() * 1.5f);
            Raylib.DrawCircleV(new Vector2(xObjeto, yObjeto), size, new Color(100, 100, 120, 150));
        }

        // Texto dentro del anillo, entre el Sol y el borde interior
        float textRadius = kuiperBeltInner * 0.7f; // 70% del radio interior
        int textWidth = Raylib.MeasureText("Cinturón de Kuiper", 12);
        Raylib.DrawText("Cinturón de Kuiper", (int)(solPantalla.X - ((float)textWidth / 2)), (int)(solPantalla.Y - textRadius), 12, new Color(100, 100, 120, 150));
    }

    private static void DrawCross(float ladoCruz, Astro astroActual, Camera camera)
    {

        double distCamPlan = Vector2D.Distance(camera.Position, astroActual.RenderPosition);
        bool objBloqueado = distCamPlan < 1e7;

        Color colorCruz;
        // Obtenemos el centro de la pantalla
        Vector2 center = camera.Center;

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
                a: (byte)(factorOpacidad * 255));
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

    private static void DrawStars(Camera camera, StarList stars)
    {
        // Calcular el área visible en coordenadas del mundo para culling temprano
        double worldWidth = camera.Width * camera.DistanceScale;
        double worldHeight = camera.Height * camera.DistanceScale;
        double margin = Math.Max(worldWidth, worldHeight) * 0.1; // 10% de margen

        double minX = camera.Position.X - (worldWidth / 2) - margin;
        double maxX = camera.Position.X + (worldWidth / 2) + margin;
        double minY = camera.Position.Y - (worldHeight / 2) - margin;
        double maxY = camera.Position.Y + (worldHeight / 2) + margin;

        // Usamos Rlgl para dibujar todas las estrellas en un solo lote (Batching)
        Rlgl.Begin(DrawMode.Quads);
        Rlgl.SetTexture(0); // Textura blanca por defecto

        foreach (var star in stars.Stars)
        {
            // Culling en coordenadas del mundo
            double starX = star.Position.X;
            double starY = star.Position.Y;

            if (starX < minX || starX > maxX || starY < minY || starY > maxY)
            {
                continue;
            }

            // Transformar de coordenadas del mundo a coordenadas de pantalla
            Vector2 posPantalla = camera.WorldToScreen(star.Position);

            byte brightness = (byte)(star.Brightness * 255);
            Rlgl.Color4ub(brightness, brightness, brightness, 255);

            float r = star.Size;

            // Dibujamos un Quad (cuadrado) centrado en la posición
            Rlgl.Vertex2f(posPantalla.X - r, posPantalla.Y - r);
            Rlgl.Vertex2f(posPantalla.X - r, posPantalla.Y + r);
            Rlgl.Vertex2f(posPantalla.X + r, posPantalla.Y + r);
            Rlgl.Vertex2f(posPantalla.X + r, posPantalla.Y - r);
        }

        Rlgl.End();
    }

    private static void DrawInfoText(Camera camera)
    {
        Raylib.DrawText(
            text: "Simulador de N-cuerpos | Pulsa Escape (Esc) para salir ",
            posX: 10,
            posY: 10,
            fontSize: 20,
            color: Color.White);
        Raylib.DrawText(
            text: "Cámara: 0 - Sol | 1 - Mercurio | 2 - Venus | ··· | 8 - Neptuno | Espacio: Visión completa del sistema",
            posX: 10,
            posY: Raylib.GetRenderHeight() - 30,
            fontSize: 20,
            color: Color.White);
        Raylib.DrawText(
            text: $"FPS {Raylib.GetFPS()}", posX: camera.Width - 200, posY: 30, fontSize: 20, color: Color.Green);
    }

    private static void DrawBodies(
        List<Astro> astros,
        Camera camera,
        Astro astroActual,
        string keyName,
        float sunRadiusAtScale,
        Vector2 posSolPantalla,
        int textAlign)
    {
        foreach (Astro astro in astros)
        {
            // Si es satélite y su planeta padre no está seleccionado, skip
            if (astro.ParentId.HasValue && (astro.ParentId.Value - astroActual.Id) != 0)
            {
                continue;
            }

            Vector2 posPantalla = camera.WorldToScreen(astro.RenderPosition);

            // Calcular la distancia en pantalla desde el Sol hasta el astro (en píxeles)
            float distanciaDesdeElSol = Vector2.Distance(posSolPantalla, posPantalla);

            // Si estamos en la vista amplia o viendo el Sol, y el astro está dentro del disco solar, ocultarlo
            if ((keyName == "Espacio" || astroActual.Id == 0) && astro.Id != 0 && distanciaDesdeElSol < sunRadiusAtScale)
            {
                continue;
            }

            // 1. Comprobamos si el astro está fuera de los límites de la pantalla
            // Creamos un rectángulo con las dimensiones de la pantalla y comprobamos si el cuerpo colisiona con él

            Rectangle screenBounds = new Rectangle(0, 0, camera.Width,
                camera.Height);
            bool enPantalla =
                Raylib.CheckCollisionPointRec(posPantalla, screenBounds);

            if (!enPantalla)
                DrawTriangles(posPantalla, camera, astro);
            else
                DrawAstro(astro, posPantalla, camera, textAlign);
        }
    }

    private static void DrawAsteroids(Camera camera, string keyName, Vector2 posSolPantalla)
    {
        // Cinturones de asteroides (centrados en el Sol)

        // Cinturón de asteroides
        DrawAsteroidBelt(posSolPantalla, camera, keyName);

        // Cinturón de Kuiper (más allá de Neptuno, desde 35 UA hasta 50 UA)
        DrawKuiperBelt(posSolPantalla, camera);
    }

    private static (float sunRadiusAtScale, Vector2 posSolPantalla) GetSun(List<Astro> astros, Camera camera)
    {
        Astro sol = astros.First(a => a.Id == 0);

        float sunRadiusAtScale = RenderSystem.GetSunRadius(sol, camera.RadiusScale);

        // Calcular la posición del Sol en pantalla (en píxeles)
        Vector2 posSolPantalla = RenderSystem.GetSunPositionScreen(sol, camera);

        return (sunRadiusAtScale, posSolPantalla);
    }

    public static void Draw(
        List<Astro> astros,
        Camera camera,
        Astro astroActual,
        float ladoCruz,
        int textAlign,
        StarList stars,
        string keyName)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);

        //Background: stars
        DrawStars(camera, stars);

        // Draw Text info
        DrawInfoText(camera);

        // Calculate sun radius and position in Screen before drawing objects
        (float sunRadiusAtScale, Vector2 posSolPantalla) = GetSun(astros, camera);

        // Asteroid belts
        DrawAsteroids(camera, keyName, posSolPantalla);

        // Drawing the different bodies of the Solar System
        DrawBodies(astros, camera, astroActual, keyName, sunRadiusAtScale, posSolPantalla, textAlign);

        // Dibujamos anillos si los tiene
        DrawRings(astroActual, camera);

        // Cruz Satelital
        DrawCross(ladoCruz, astroActual, camera);
    }
}