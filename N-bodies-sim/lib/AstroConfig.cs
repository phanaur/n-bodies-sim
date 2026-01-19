using Raylib_cs;

namespace N_bodies_sim;

public readonly struct AstroConfig
{
    public required KeyboardKey Key { get; init; }
    public required string Name { get; init; }
    public required string KeyName { get; init; }

    public required float Id { get; init; }

    public required int TextAlign { get; init; }

    public required double TargetDistanceScale { get; init; }

    public required double TargetRadiusScale { get; init; }
    public required double TargetTimeStep { get; init; }

    public required double TargetN { get; init; }

    public required double InitialLerpSpeed { get; init; }
}