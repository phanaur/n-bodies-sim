/*
 * This class is responsible for loading the information of all bodies displayed in the Solar System, obtaining the
 * information from the Data/astrosData.json file, located at the project root. It generates a list of Astro objects with
 * the necessary information.
 */
using System.Text.Json;
using NBodiesSim.Source.Core;
using NBodiesSim.Source.Models;
using NBodiesSim.Source.Models.DTOs;
using Raylib_cs;

namespace NBodiesSim.Source.Data;

public class DataLoader
{
    // List of astros
    public readonly List<Astro> Astros = new List<Astro>();

    // Constructor
    public DataLoader()
    {
        const string filePath = "Data/astrosData.json";

        // Check if the file exists
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        // Generate astros
        string jsonAstroData = File.ReadAllText(filePath);
        WrapperAstroData? wrapperAstroData = JsonSerializer.Deserialize<WrapperAstroData>(jsonAstroData);

        // Check deserialization
        if (wrapperAstroData?.Astros == null)
        {
            throw new Exception("Error loading JSON file or it is empty");
        }

        // Iterate over the deserialized astro list
        foreach (AstroData data in wrapperAstroData.Astros)
        {
            double desiredTrailTime = data.DesiredTrailTime;

            // If it's a satellite, inherit DesiredTrailTime from its parent planet
            if (data.ParentId != null)
            {
                AstroData? parentData = wrapperAstroData.Astros.FirstOrDefault(p => (p.Id - data.ParentId) == 0);
                if (parentData != null) desiredTrailTime = parentData.DesiredTrailTime;
            }

            // Check if it has rings
            Color? ringColor = null;
            if (data is { HasRings: true, RingColor: not null })
            {
                ringColor = new Color(data.RingColor[0], data.RingColor[1], data.RingColor[2], data.RingColor[3]);
            }

            // Create the Astro object for each body
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
                DesiredTrailTime = desiredTrailTime,
                Trail = new Queue<Vector2D>(),
                HasRings = data.HasRings,
                InnerRingRadius = data.InnerRingRadius,
                OuterRingRadius = data.OuterRingRadius,
                RingColor = ringColor,
            };
            Astros.Add(astro); // Add to astro list
        }
    }
}