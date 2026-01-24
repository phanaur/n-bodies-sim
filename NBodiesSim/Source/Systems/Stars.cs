using NBodiesSim.Source.Core;
using NBodiesSim.Source.Models;

namespace NBodiesSim.Source.Systems;

public class StarList
{


    public List<(Vector2D Position, float Brightness, float Size)> Stars { get; set; } =
        new List<(Vector2D Position, float Brightness, float Size)>();


    public void GenerateStars()
    {
        Random rndStars = new Random(12345); // Fixed seed for consistency

        for (int i = 0; i < StarsConstants.NumStars; i++)
        {
            // Generate random angle
            double angulo = rndStars.NextDouble() * 2 * Math.PI;

            // Random distance very far away (beyond the Kuiper belt: 1e13 to 5e13 meters)
            double distancia = rndStars.NextDouble() * 4e13;

            // Position in world coordinates
            double x = Math.Cos(angulo) * distancia;
            double y = Math.Sin(angulo) * distancia;

            float brightness = (float)(0.3 + (rndStars.NextDouble() * 0.7));
            float size = (float)(0.5 + (rndStars.NextDouble() * 1.5));
            Stars.Add((new Vector2D(x, y), brightness, size));
        }
    }
}