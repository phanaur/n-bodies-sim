using NBodiesSim.Source;
using NBodiesSim.Source.Data;
using NBodiesSim.Source.Systems;

namespace NBodiesSim;

internal static class Program
{
    private static void Main()
    {
        // 1. Crear todas las dependencias
        var physics = new PhysicsEngine();
        var render = new RenderSystem();
        var data = new DataLoader();
        var config = new ConfigLoader();
        
        // 2. Crear el InputSystem, que depende de la configuración
        var input = new InputSystems(config.CameraConf);
        
        // 3. Crear la simulación, pasándole todos los sistemas
        var sim = new Simulation(physics, render, data, input);
        
        // 4. Ejecutar la simulación
        sim.Run();
    }
}