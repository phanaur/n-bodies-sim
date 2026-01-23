/*
 * Esta clase se encarga de cargar la información de todos los cuerpos que se muestran en el Sistema Solar, obteniendo la
 * información del archivo Data/astrosData.json, situado en la raíz del proyecto. Genera una lista de objetos Astro, con
 * la información necesaria.
 */
using System.Text.Json;
using NBodiesSim.Source.Core;
using NBodiesSim.Source.Models;
using NBodiesSim.Source.Models.DTOs;
using Raylib_cs;

namespace NBodiesSim.Source.Data;

public class DataLoader
{
    // Lista de astros
    public readonly List<Astro> Astros = new List<Astro>();

    // Constructor
    public DataLoader()
    {

        // Generación de astros
        string jsonAstroData = File.ReadAllText("Data/astrosData.json");
        WrapperAstroData? wrapperAstroData = JsonSerializer.Deserialize<WrapperAstroData>(jsonAstroData);

        // Comprobación de la deserialización.
        if (wrapperAstroData?.Astros == null)
        {
            throw new Exception("Error al cargar el archivo JSON o está vacío");
        }

        // Iteración sobre la lista de astros deserializados
        foreach (AstroData data in wrapperAstroData.Astros)
        {
            int trailLength = data.TrailLength;

            // Si es satélite, heredará el TrailLength de su planeta padre
            if (data.ParentId != null)
            {
                AstroData? parentData = wrapperAstroData.Astros.FirstOrDefault(p => (p.Id - data.ParentId) == 0);
                if (parentData != null) trailLength = parentData.TrailLength;
            }

            // Comprobamos si tiene anillos
            Color? ringColor = null;
            if (data is { HasRings: true, RingColor: not null })
            {
                ringColor = new Color(data.RingColor[0], data.RingColor[1], data.RingColor[2], data.RingColor[3]);
            }

            // Creación del objeto Astro para cada cuerpo
            var astro = new Astro
            {
                Id = data.Id,
                ParentId = data.ParentId,
                Name = data.Name,
                Mass = data.Mass,
                Radius = data.Radius,
                Color = new Color(data.Color[0], data.Color[1], data.Color[2], data.Color[3]),
                Position = new Vector2D(data.Position[0], data.Position[1]),
                PastPosition = new Vector2D(data.Position[0], data.Position[1]),
                RenderPosition = new Vector2D(data.Position[0], data.Position[1]),
                Velocity = new Vector2D(data.Velocity[0], data.Velocity[1]),
                Acceleration = Vector2D.Zero,
                TrailLength = trailLength,
                Trail = new Queue<Vector2D>(),
                HasRings = data.HasRings,
                InnerRingRadius = data.InnerRingRadius,
                OuterRingRadius = data.OuterRingRadius,
                RingColor = ringColor,
            };
            Astros.Add(astro); // se añade a la lista de astros
        }
    }
}