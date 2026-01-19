class AstroData
{
    public required float Id { get; set; }

    public float? ParentId { get; set; }
    public required string Name { get; set; }
    public required double Mass { get; set; }
    public required double Radius { get; set; }
    public required int[] Color { get; set; }  // [r, g, b, a]
    public required double[] Position { get; set; }  // [x, y]
    public required double[] Velocity { get; set; }  // [x, y]
    public required int TrailLength { get; set; }

    // En caso de tener anillos (hecho entero por la IA):
    public bool HasRings { get; set; } = false;
    public double InnerRingRadius { get; set; }  // en metros
    public double OuterRingRadius { get; set; }  // en metros
    public int[]? RingColor { get; set; }  // [r, g, b, a]
}