using NBodiesSim.Source.Core;
using NBodiesSim.Source.Data;
using NBodiesSim.Source.Models;
using NBodiesSim.Source.Systems;
using Raylib_cs;

namespace NBodiesSim.Source;

internal class Simulation
{
  // Mercury visibility condition
  private static string _keyName = "Null";

  // 4. Initial configuration setup
  private double _timeStep = 43200; // Each simulation frame represents one Earth day in real time.
  private double _n = 300; // Number of calculations into which timeStep will be divided. Higher means better precision.
  private int _textAlign = SimulationConstants.DefaultTextAlign;

  private readonly StarList _stars = new StarList();
  private readonly Camera _camera = new Camera();
  private Astro? _selectedAstro;

  private readonly PhysicsEngineRk4 _physicsEngine;
  private readonly RenderSystem _renderSystem;
  private readonly DataLoader _dataLoader;
  private readonly InputSystems _inputSystems;
  //private readonly SimulationLogger _logger;

  //Variables for loggin when necessary
  private double _simulatedTime;
  //private double _lastLogTime;
  //private const double LogInterval = 360;
  private (double, double, double, double) _lastEnergyCalc = (0, 0, 0, 0);


  public Simulation(
      PhysicsEngineRk4 physicsEngine,
      RenderSystem renderSystem,
      DataLoader dataLoader,
      InputSystems inputSystems
  )
  {
    _physicsEngine = physicsEngine;
    _renderSystem = renderSystem;
    _dataLoader = dataLoader;
    _inputSystems = inputSystems;
  }

  public void Initialize()
  {

    // 1. Create the Raylib window
    Raylib.SetConfigFlags(ConfigFlags.FullscreenMode | ConfigFlags.Msaa4xHint);
    Raylib.InitWindow(_camera.Width, _camera.Height, "N-Bodies Simulator");
    Raylib.SetTargetFPS(Raylib.GetMonitorRefreshRate(0));

    // Generate initial stars (only once)
    _stars.GenerateStars();
  }

  public void Run()
  {
    List<Astro> astros = _dataLoader.Astros;
    _selectedAstro = astros.First(a => (a.Id - _camera.TargetId) == 0);

    // Variables for the independent physics calculations
    double accumulator = 0;

    // Warm-up: run physics once to initialize PastPosition
    _physicsEngine.UpdateRk4(astros, _timeStep / _n);

    // 5. Main loop
    while (!Raylib.WindowShouldClose())
    {
      double dt = Raylib.GetFrameTime();
      var newConfig = _inputSystems.ProcessInput();
      if (newConfig != null)
      {
        _camera.TargetId = newConfig.Value.Id;
        _camera.TargetDistanceScale = newConfig.Value.TargetDistanceScale;
        _camera.TargetRadiusScale = newConfig.Value.TargetRadiusScale;
        _timeStep = newConfig.Value.TargetTimeStep;
        _n = newConfig.Value.TargetN;
        _camera.ResetLerp();
        _textAlign = newConfig.Value.TextAlign;
        _keyName = newConfig.Value.KeyName;

        // Update the current astro only when the target changes
        _selectedAstro = astros.First(a => (a.Id - _camera.TargetId) == 0);
      }

      // Physics: Once per frame, independent of FPS
      accumulator += dt;

      while (accumulator >= SimulationConstants.FixedDt)
      {
        for (int i = 0; i < _n; i++)
        {
          _physicsEngine.UpdateRk4(_dataLoader.Astros, _timeStep / _n);
          _simulatedTime += _timeStep / _n;
        }
        //if (_simulatedTime - _lastLogTime >= LogInterval)
        //{
        //  (double, double, double, double) energyCalc = _physicsEngine.CalculateEnergy(_dataLoader.Astros);
        //  _lastEnergyCalc = energyCalc;
        //  _logger.LogFrame(_simulatedTime, energyCalc);
        //  _lastLogTime = _simulatedTime;
        //}
        (double, double, double, double) energyCalc = _physicsEngine.CalculateEnergy(_dataLoader.Astros);
        _lastEnergyCalc = energyCalc;

        accumulator -= SimulationConstants.FixedDt;
      }

      _renderSystem.SaveTrail(astros, _timeStep);

      _camera.Update(dt, _selectedAstro.Position);



      // Draw frame
      _renderSystem.Draw(
          astros,
          _camera,
          _selectedAstro,
          SimulationConstants.CrossSideLength,
          _textAlign,
          _stars,
          _keyName,
          _physicsEngine,
          _lastEnergyCalc,
          _simulatedTime
      );
    }
    //_logger.Dispose();
    Raylib.CloseWindow();
  }
}
