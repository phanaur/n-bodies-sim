/*
 * Esta clase contiene la información para la generación de los objetos de tipo AstroConfig, con todos los parámetros
 * necesarios para ajustar la cámara, el cálculo del renderizado y el número de cálculos que realiza el motor de físicas.
 */
using Raylib_cs;

namespace NBodiesSim.Source.Models.DTOs;

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