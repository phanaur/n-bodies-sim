using NBodiesSim.Source.Core;

namespace NBodiesSim.Source.Systems;

public class StarList
{


    public List<(Vector2D Position, float Brightness, float Size)> Stars { get; set; } =
        new List<(Vector2D Position, float Brightness, float Size)>();


    public void GenerateStars()
    {
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

            float brightness = (float)(0.3 + (rndStars.NextDouble() * 0.7));
            float size = (float)(0.5 + (rndStars.NextDouble() * 1.5));
            Stars.Add((new Vector2D(x, y), brightness, size));
        }
    }
}