using NBodiesSim.Source;
using NBodiesSim.Source.Data;
using NBodiesSim.Source.Systems;

namespace NBodiesSim;

internal static class Program
{
    private static void Main()
    {
        // 1. Crear todas las dependencias
        PhysicsEngine physics = new PhysicsEngine();
        RenderSystem render = new RenderSystem();
        DataLoader data = new DataLoader();
        ConfigLoader config = new ConfigLoader();
        
        // 2. Crear el InputSystem, que depende de la configuración
        InputSystems input = new InputSystems(config.CameraConf);
        
        // 3. Crear la simulación, pasándole todos los sistemas
        Simulation sim = new Simulation(physics, render, data, input);
        
        // 4. Ejecutar la simulación
        sim.Run();
    }
}