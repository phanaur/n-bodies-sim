using NBodiesSim.Source.Data;
using NBodiesSim.Source.Core;
using NBodiesSim.Source.Models;
using NBodiesSim.Source.Systems;
using Raylib_cs;

namespace NBodiesSim.Source;

public class Simulation
{
    // Condición visibilidad Mercurio
    private static string _keyName = "Null";

    // 4. Establecimiento de la configuración inicial
    private double _timeStep = 86400; // Cada frame de la simulación significa un día terrestre en tiempo real.
    private int _n = 1000; // Número de cálculos en los que se dividirá timeStep. Cuanto mayor sea, mejor precisión.

    private double _targetTimeStep = 86400; // Objetivo de timeStep
    private double _targetN = 1000; // Objetivo de n

    private int _textAlign = 40;
    private const float LadoCruz = 10;

    private readonly StarList _stars = new StarList();
    private readonly Camera _camera = new Camera();
    private Astro? _astroActual;


    private readonly PhysicsEngine _physicsEngine;
    private readonly RenderSystem _renderSystem;
    private readonly DataLoader _dataLoader;
    private readonly InputSystems _inputSystems;

    public Simulation(
        PhysicsEngine physicsEngine,
        RenderSystem renderSystem,
        DataLoader dataLoader,
        InputSystems inputSystems)
    {
        _physicsEngine = physicsEngine;
        _renderSystem = renderSystem;
        _dataLoader = dataLoader;
        _inputSystems = inputSystems;

        // 1. Crear la ventana en Raylib
        Raylib.SetConfigFlags(ConfigFlags.FullscreenMode | ConfigFlags.Msaa4xHint);
        Raylib.InitWindow(_camera.Width, _camera.Height, "Simulador de N-cuerpos");
        Raylib.SetTargetFPS(Raylib.GetMonitorRefreshRate(0));

        // Generar estrellas iniciales (solo una vez)
        _stars.GenerateStars();
    }

    public void Run()
    {
        List<Astro> astros = _dataLoader.Astros;
        _astroActual = astros.First(a => (a.Id - _camera.TargetId) == 0);

        // 5. Bucle principal
        while (!Raylib.WindowShouldClose())
        {
            double dt = Raylib.GetFrameTime();
            var newConfig = _inputSystems.ProcessInput();
            if (newConfig != null)
            {
                _camera.TargetId = newConfig.Value.Id;
                _camera.TargetDistanceScale = newConfig.Value.TargetDistanceScale;
                _camera.TargetRadiusScale = newConfig.Value.TargetRadiusScale;
                _targetTimeStep = newConfig.Value.TargetTimeStep;
                _targetN = newConfig.Value.TargetN;
                _camera.ResetLerp();
                _textAlign = newConfig.Value.TextAlign;
                _keyName = newConfig.Value.KeyName;
                
                // Actualizar el astro actual solo cuando cambia el objetivo
                _astroActual = astros.First(a => (a.Id - _camera.TargetId) == 0);
            }
            // Física: Una vez por frame, independiente de FPS
            double scaledTimeStep = _timeStep * dt * 60.0; // Normalizado a 60 FPS
            _physicsEngine.UpdatePhysics(astros, scaledTimeStep, _n);
            RenderSystem.SaveTrail(astros);

            // Interpolación visual (suave, basada en dt para ser independiente de FPS)
            double smoothFactor = 1.0 - Math.Pow(1.0 - _camera.LerpSpeed, dt * 60.0);

            _timeStep += (_targetTimeStep - _timeStep) * smoothFactor;
            _n += (int)((_targetN - _n) * smoothFactor);

            _camera.Update(dt, _astroActual.Position);

            // Dibujo del frame
            RenderSystem.Draw(
                astros,
                _camera,
                _astroActual,
                LadoCruz,
                _textAlign,
                _stars,
                _keyName);
        }
        Raylib.CloseWindow();
    }
}